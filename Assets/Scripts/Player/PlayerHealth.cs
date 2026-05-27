using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using CODEX.Systems;
using CODEX.Enemies;

namespace CODEX.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerHealth : MonoBehaviour, Projectile.IDamageable
    {
        [Header("Salud")]
        [SerializeField] private int maxHP = 5;
        [SerializeField] private int respawnHP = 3;             // REFACTOR: respawn a 3/5 según diseño tutorial
        [SerializeField] private float invincibilityDuration = 1f;
        [SerializeField] private float knockbackForce = 6f;
        [SerializeField] private float respawnDelay = 1.5f;

        [Header("Contacto con Enemigos")]
        [SerializeField] private int contactDamage = 1;

        [Header("Checkpoint")]
        [SerializeField] private Transform currentCheckpoint;

        [Header("Visual - Gafas")]                              // REFACTOR: absorbido de HealthSystem
        [SerializeField] private SpriteRenderer gogglesRenderer;
        private static readonly Color ColorFull = new Color(0.2f, 0.6f, 1f);   // azul
        private static readonly Color ColorMid  = new Color(1f, 0.85f, 0.1f);  // amarillo
        private static readonly Color ColorLow  = new Color(1f, 0.2f, 0.2f);   // rojo

        [Header("Eventos")]
        public UnityEvent<int> OnDamaged;
        public UnityEvent OnDied;

        // REFACTOR: reemplaza HealthSystem.OnHealthChanged — firma (current, max)
        public System.Action<int, int> OnHealthChanged;

        /// <summary>
        /// Evento global: se dispara cada vez que el jugador reaparece.
        /// Cualquier FallingPlatform, puerta, etc. puede suscribirse para resetearse.
        /// </summary>
        public static System.Action OnPlayerRespawned;

        private int currentHP;
        private bool isDead;
        private PlayerController playerController;
        private CameraFollow cameraFollow;

        public int CurrentHP       => currentHP;
        public int MaxHP           => maxHP;
        public bool IsDead         => isDead;
        public bool IsAlive        => currentHP > 0;            // REFACTOR: absorbido de HealthSystem
        public float HealthPercent => maxHP > 0 ? (float)currentHP / maxHP : 0f; // REFACTOR: absorbido de HealthSystem

        // ═══════════════════════════════════════════
        //  UNITY LIFECYCLE
        // ═══════════════════════════════════════════

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
        }

        private void Start()
        {
            currentHP = maxHP;
            cameraFollow = FindAnyObjectByType<CameraFollow>();
            UpdateGogglesColor();                               // REFACTOR: estado inicial de gafas
            OnHealthChanged?.Invoke(currentHP, maxHP);         // REFACTOR: inicializar HUD al arrancar
        }

        // ═══════════════════════════════════════════
        //  DAÑO — IDamageable (proyectiles)
        // ═══════════════════════════════════════════

        public void TakeDamage(int damage, Vector2 sourcePosition)
        {
            if (isDead || playerController.IsInvincible) return;

            currentHP = Mathf.Max(0, currentHP - damage);
            OnDamaged?.Invoke(currentHP);
            OnHealthChanged?.Invoke(currentHP, maxHP);         // REFACTOR: notificar HUD y suscriptores
            UpdateGogglesColor();                               // REFACTOR: actualizar gafas en cada golpe

            Vector2 knockDir = ((Vector2)transform.position - sourcePosition).normalized;
            playerController.ApplyKnockback(knockDir, knockbackForce);
            playerController.SetInvincible(invincibilityDuration);

            if (currentHP <= 0)
                StartCoroutine(DieAndRespawn());
        }

        // ═══════════════════════════════════════════
        //  CONTACTO CON CUERPO DE ENEMIGO
        // ═══════════════════════════════════════════

        // OnTriggerEnter2D: cubre enemigos que usen hitbox-trigger separado
        private void OnTriggerEnter2D(Collider2D other)
        {
            var root = other.transform.root;
            var enemy = root.GetComponent<InfectedFile>()
                     ?? root.GetComponentInChildren<InfectedFile>();
            if (enemy != null && enemy.IsActive)
                TakeDamage(contactDamage, other.transform.position);
        }

        // OnCollisionEnter2D: cubre enemigos con collider físico normal (sin trigger)
        private void OnCollisionEnter2D(Collision2D collision)
        {
            var root = collision.transform.root;
            var enemy = root.GetComponent<InfectedFile>()
                     ?? root.GetComponentInChildren<InfectedFile>();
            if (enemy != null && enemy.IsActive)
                TakeDamage(contactDamage, collision.transform.position);
        }

        // ═══════════════════════════════════════════
        //  CHECKPOINTS
        // ═══════════════════════════════════════════

        public void SetCheckpoint(Transform checkpoint)
        {
            currentCheckpoint = checkpoint;
        }

        // ═══════════════════════════════════════════
        //  REVIVE EXTERNO (pickups, terminales, etc.)
        // ═══════════════════════════════════════════

        /// <summary>Muerte instantánea — ignora invencibilidad. Úsalo en DeathZone y abismos.</summary>
        public void Kill()
        {
            if (isDead) return;
            currentHP = 0;
            OnHealthChanged?.Invoke(0, maxHP);
            StartCoroutine(DieAndRespawn());
        }

        public void Revive(int withHP = -1)                    // REFACTOR: absorbido de HealthSystem
        {
            currentHP = withHP > 0 ? Mathf.Min(withHP, maxHP) : maxHP;
            isDead = false;
            OnHealthChanged?.Invoke(currentHP, maxHP);
            UpdateGogglesColor();
        }

        // ═══════════════════════════════════════════
        //  MUERTE Y RESPAWN
        // ═══════════════════════════════════════════

        private IEnumerator DieAndRespawn()
        {
            if (isDead) yield break;
            isDead = true;

            playerController.SetInputEnabled(false);
            OnDied?.Invoke();

            yield return new WaitForSeconds(respawnDelay);

            Respawn();
        }

        private void Respawn()
        {
            currentHP = Mathf.Clamp(respawnHP, 1, maxHP);     // FIX: era maxHP; ahora usa respawnHP (3)
            isDead = false;

            if (currentCheckpoint != null)
                playerController.TeleportTo(currentCheckpoint.position);

            cameraFollow?.SnapToTarget();
            playerController.SetInputEnabled(true);
            playerController.SetInvincible(invincibilityDuration);

            OnHealthChanged?.Invoke(currentHP, maxHP);         // REFACTOR: notificar HUD al respawnear
            UpdateGogglesColor();                               // REFACTOR: resetear color de gafas
            OnPlayerRespawned?.Invoke();                        // Notificar plataformas, puertas, etc.
        }

        // ═══════════════════════════════════════════
        //  VISUAL - GAFAS
        // ═══════════════════════════════════════════

        private void UpdateGogglesColor()                      // REFACTOR: absorbido de HealthSystem
        {
            if (gogglesRenderer == null) return;
            if (HealthPercent > 0.6f)      gogglesRenderer.color = ColorFull;
            else if (HealthPercent > 0.2f) gogglesRenderer.color = ColorMid;
            else                           gogglesRenderer.color = ColorLow;
        }
    }
}
