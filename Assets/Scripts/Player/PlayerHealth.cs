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
        [SerializeField] private float invincibilityDuration = 1f;
        [SerializeField] private float knockbackForce = 6f;
        [SerializeField] private float respawnDelay = 1.5f;

        [Header("Contacto con Enemigos")]
        [SerializeField] private int contactDamage = 1;

        [Header("Checkpoint")]
        [SerializeField] private Transform currentCheckpoint;

        [Header("Eventos")]
        public UnityEvent<int> OnDamaged;
        public UnityEvent OnDied;

        private int currentHP;
        private bool isDead;
        private PlayerController playerController;
        private CameraFollow cameraFollow;

        public int CurrentHP => currentHP;
        public int MaxHP => maxHP;
        public bool IsDead => isDead;

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
            cameraFollow = FindFirstObjectByType<CameraFollow>();
        }

        // ═══════════════════════════════════════════
        //  DAÑO — IDamageable (proyectiles)
        // ═══════════════════════════════════════════

        public void TakeDamage(int damage, Vector2 sourcePosition)
        {
            if (isDead || playerController.IsInvincible) return;

            currentHP = Mathf.Max(0, currentHP - damage);
            OnDamaged?.Invoke(currentHP);

            Vector2 knockDir = ((Vector2)transform.position - sourcePosition).normalized;
            playerController.ApplyKnockback(knockDir, knockbackForce);
            playerController.SetInvincible(invincibilityDuration);

            if (currentHP <= 0)
                StartCoroutine(DieAndRespawn());
        }

        // ═══════════════════════════════════════════
        //  CONTACTO CON CUERPO DE ENEMIGO
        // ═══════════════════════════════════════════

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<InfectedFile>(out var enemy) && enemy.IsActive)
                TakeDamage(contactDamage, other.transform.position);
        }

        // ═══════════════════════════════════════════
        //  CHECKPOINTS
        // ═══════════════════════════════════════════

        public void SetCheckpoint(Transform checkpoint)
        {
            currentCheckpoint = checkpoint;
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
            currentHP = maxHP;
            isDead = false;

            if (currentCheckpoint != null)
                playerController.TeleportTo(currentCheckpoint.position);

            cameraFollow?.SnapToTarget();
            playerController.SetInputEnabled(true);
            playerController.SetInvincible(invincibilityDuration);

            OnDamaged?.Invoke(currentHP);
        }
    }
}
