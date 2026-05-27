using UnityEngine;
using UnityEngine.Events;

namespace CODEX.Enemies
{
    public class InfectedFile : MonoBehaviour, CODEX.Systems.Projectile.IDamageable
    {
        [System.Serializable]
        public class EnemyEvent : UnityEvent<InfectedFile> { }
        
        [System.Serializable]
        public enum EnemyType
        {
            TypeA_Static,      // No se mueve, fácil de disparar (Bloque 2, tutorial)
            TypeB_Patrol,      // Se mueve de lado a lado, requiere timing (Bloque 7)
            TypeC_Projectile,  // Dispara rayos lentos, requiere esquive (Bloque 7)
            TypeD_Melee,       // Ataque cuerpo a cuerpo
            // P7 FIX: TypeE_Special eliminado — no existe en el juego, no tiene implementación.
            // Si se necesita un boss en el futuro, crear EnemyType en un archivo separado.
        }
        
        [Header("Configuración del Enemigo")]
        [SerializeField] private EnemyType enemyType = EnemyType.TypeA_Static;
        [SerializeField] private int maxHealth = 3;
#pragma warning disable CS0414
        [SerializeField] private int damageToPlayer = 1;
#pragma warning restore CS0414
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private bool dropsData = true;
        [SerializeField] private int dataDropAmount = 1;
        [SerializeField] private GameObject dataDropPrefab;
        
        [Header("IA y Movimiento")]
        [SerializeField] private float patrolDistance = 5f;
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private bool loopPatrol = true;
        private int currentPatrolIndex = 0;
        private Rigidbody2D rb;
        private Vector2 startPosition;
        private float patrolDirection = 1f;
#pragma warning disable CS0414
        private bool isPatrolling = false;
#pragma warning restore CS0414
        
        [Header("Ataque a Distancia")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private float projectileDamage = 1f;
        private float attackTimer = 0f;
        
        [Header("Visuales y Efectos")]
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private float corruptionGlowIntensity = 1f;
        [SerializeField] private float corruptionPulseSpeed = 2f;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Material enemyMaterial;
        
        [Header("Estado")]
        [SerializeField] private int currentHealth;
        private bool isDead = false;
        private bool isAggroed = false;
        private Transform playerTransform;
        private float stunTimer = 0f;
        
        [Header("Eventos")]
        public EnemyEvent OnEnemyDeath = new EnemyEvent();
        public UnityEvent<int> OnHealthChanged = new UnityEvent<int>();
        public UnityEvent OnPlayerDetected = new UnityEvent();
        public UnityEvent OnAttack = new UnityEvent();
        
        // === PROPIEDADES PÚBLICAS ===
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => isDead;
        public float HealthPercentage => (float)currentHealth / maxHealth;
        public EnemyType Type => enemyType;
        public bool IsActive => !isDead;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
                
                // Obtener material para efectos de shader (opcional)
                if (spriteRenderer.material != null)
                {
                    enemyMaterial = spriteRenderer.material;
                }
            }
            
            startPosition = transform.position;
            
            // Buscar jugador
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            // Configurar según tipo
            ConfigureForType();
        }
        
        private void Start()
        {
            currentHealth = maxHealth;
            
            // Iniciar patrulla si corresponde
            if (enemyType == EnemyType.TypeB_Patrol && patrolPoints.Length > 0)
            {
                StartPatrolling();
            }
        }
        
        private void Update()
        {
            if (isDead) return;
            
            // Actualizar timers
            attackTimer -= Time.deltaTime;
            stunTimer -= Time.deltaTime;
            
            bool isStunned = stunTimer > 0f;
            
            // Comportamiento según tipo
            switch (enemyType)
            {
                case EnemyType.TypeA_Static:
                    // No hace nada, solo espera a ser disparado
                    UpdateStaticBehavior();
                    break;
                    
                case EnemyType.TypeB_Patrol:
                    UpdatePatrolBehavior(isStunned);
                    break;
                    
                case EnemyType.TypeC_Projectile:
                    UpdateProjectileBehavior(isStunned);
                    break;
                    
                case EnemyType.TypeD_Melee:
                    UpdateMeleeBehavior(isStunned);
                    break;
                    
                default:
                    break;
            }
            
            // Actualizar efectos visuales
            UpdateVisualEffects();
        }
        
        // === COMPORTAMIENTOS POR TIPO ===
        
        private void ConfigureForType()
        {
            switch (enemyType)
            {
                case EnemyType.TypeA_Static:
                    // Configuración para tutorial - no se mueve
                    if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
                    break;
                    
                case EnemyType.TypeB_Patrol:
                    // Mueve de lado a lado
                    if (patrolPoints.Length == 0)
                    {
                        CreateDefaultPatrolPoints();
                    }
                    break;
                    
                case EnemyType.TypeC_Projectile:
                    // Construir punto de disparo si no existe
                    if (firePoint == null)
                    {
                        GameObject firePointObj = new GameObject("FirePoint");
                        firePointObj.transform.SetParent(transform);
                        firePointObj.transform.localPosition = new Vector2(1f, 0.5f);
                        firePoint = firePointObj.transform;
                    }
                    break;
            }
        }
        
        private void UpdateStaticBehavior()
        {
            // No hace nada, solo palpita con efectos de corrupción
            // Ideal para el tutorial Bloque 2
        }
        
        private void UpdatePatrolBehavior(bool isStunned)
        {
            if (isStunned) return;

            // ── Detección: perseguir si el jugador está en rango, patrullar si no ──
            // Usa attackRange como radio de detección (configura en el Inspector).
            bool playerDetected = playerTransform != null &&
                                  Vector2.Distance(transform.position, playerTransform.position) < attackRange;

            if (playerDetected)
            {
                isAggroed = true;
                // Solo movimiento horizontal — el enemigo permanece en la plataforma
                float dirX = Mathf.Sign(playerTransform.position.x - transform.position.x);
                if (rb != null)
                    rb.linearVelocity = new Vector2(dirX * moveSpeed * 1.5f, rb.linearVelocity.y);
            }
            else
            {
                isAggroed = false;

                // Patrullar entre waypoints asignados
                if (patrolPoints != null && patrolPoints.Length > 0)
                {
                    Transform target = patrolPoints[currentPatrolIndex];
                    float dirX = Mathf.Sign(target.position.x - transform.position.x);
                    if (rb != null)
                        rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);

                    if (Mathf.Abs(transform.position.x - target.position.x) < 0.5f)
                        currentPatrolIndex = GetNextPatrolIndex();
                }
                else
                {
                    // Patrulla simple de lado a lado usando patrolDistance
                    if (transform.position.x > startPosition.x + patrolDistance)
                        patrolDirection = -1f;
                    else if (transform.position.x < startPosition.x - patrolDistance)
                        patrolDirection = 1f;

                    if (rb != null)
                        rb.linearVelocity = new Vector2(patrolDirection * moveSpeed, rb.linearVelocity.y);
                }
            }

            // Voltear sprite según dirección horizontal
            if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(rb.linearVelocity.x);
                transform.localScale = scale;
            }
        }
        
        private void UpdateProjectileBehavior(bool isStunned)
        {
            if (isStunned) return;
            
            // Perseguir al jugador
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
                
                if (distanceToPlayer < 10f && !isAggroed)
                {
                    isAggroed = true;
                    OnPlayerDetected.Invoke();
                }
                
                if (isAggroed && distanceToPlayer > 15f)
                {
                    isAggroed = false;
                }
                
                // Atacar si está en rango
                if (isAggroed && distanceToPlayer < attackRange && attackTimer <= 0f)
                {
                    AttackPlayer();
                    attackTimer = attackCooldown;
                }
                
                // Moverse hacia el jugador si no está muy cerca
                if (isAggroed && distanceToPlayer > 3f)
                {
                    Vector2 direction = (playerTransform.position - transform.position).normalized;
                    if (rb != null)
                    {
                        rb.linearVelocity = direction * moveSpeed;
                        
                        // Rotar sprite según dirección
                        if (rb.linearVelocity.x != 0)
                        {
                            Vector3 scale = transform.localScale;
                            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(rb.linearVelocity.x);
                            transform.localScale = scale;
                        }
                    }
                }
            }
        }
        
        private void UpdateMeleeBehavior(bool isStunned)
        {
            if (isStunned) return;
            
            // Lógica similar a patrol pero con ataque cuerpo a cuerpo
            UpdatePatrolBehavior(isStunned);
            
            // Ataque cuerpo a cuerpo si el jugador está cerca
            if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) < 1.5f && attackTimer <= 0f)
            {
                AttackMelee();
                attackTimer = attackCooldown;
            }
        }
        
        // === ATAQUES ===
        
        private void AttackPlayer()
        {
            if (isDead || projectilePrefab == null || firePoint == null) return;
            
            // Crear proyectil
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            
            // Dirección hacia el jugador
            Vector2 direction = (playerTransform.position - firePoint.position).normalized;
            
            CODEX.Systems.Projectile projectileScript = projectile.GetComponent<CODEX.Systems.Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(direction, projectileSpeed, Mathf.RoundToInt(projectileDamage), 
                                          LayerMask.GetMask("Player"), 5f);
            }
            
            OnAttack.Invoke();
        }
        
        private void AttackMelee()
        {
            if (isDead || playerTransform == null) return;
            
            // Aplicar daño al jugador
            // CODEX.Systems.HealthSystem playerHealth = playerTransform.GetComponent<CODEX.Systems.HealthSystem>();
            // if (playerHealth != null)
            // {
            //     playerHealth.TakeDamage(damageToPlayer, transform.position);
            // }
            
            OnAttack.Invoke();
            
            // Efecto de empuje/knockback
            Vector2 knockbackDirection = (playerTransform.position - transform.position).normalized;
            Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
            }
        }
        
        // === DAÑO Y SALUD ===
        
        public void TakeDamage(int damage, Vector2 sourcePosition)
        {
            if (isDead) return;
            
            // Reducir salud
            currentHealth = Mathf.Max(0, currentHealth - damage);
            OnHealthChanged.Invoke(currentHealth);
            
            // Efecto visual
            FlashDamage();
            
            // Knockback/Stun según tipo
            ApplyDamagedReaction(sourcePosition);
            
            // Chequear muerte
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        private void ApplyDamagedReaction(Vector2 sourcePosition)
        {
            // Aplicar stun y knockback
            stunTimer = 0.3f; // Breve stun al recibir daño
            
            // Knockback opuesto a la fuente
            if (rb != null)
            {
                Vector2 knockbackDirection = (transform.position - (Vector3)sourcePosition).normalized;
                rb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
            }
            
            // Particular para TypeA_Static (Tutorial): no se mueve ni reacciona
            if (enemyType == EnemyType.TypeA_Static)
            {
                // En el tutorial, el enemigo no reacciona al daño
                stunTimer = 0f;
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }
        
        // === MUERTE ===
        
        private void Die()
        {
            if (isDead) return;
            
            isDead = true;
            
            // Desactivar componentes físicos
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            
            // Efecto de muerte
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Soltar datos si corresponde
            if (dropsData && dataDropPrefab != null)
            {
                for (int i = 0; i < dataDropAmount; i++)
                {
                    Vector2 dropOffset = Random.insideUnitCircle * 0.5f;
                    Instantiate(dataDropPrefab, transform.position + (Vector3)dropOffset, Quaternion.identity);
                }
            }
            
            // Eventos
            OnEnemyDeath.Invoke(this);
            
            // Desactivar después de un tiempo
            Destroy(gameObject, 2f);
            
            // Log para depuración
            Debug.Log($"Archivo Infectado destruido! Tipo: {enemyType}");
        }
        
        // === EFECTOS VISUALES ===
        
        private void FlashDamage()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = flashColor;
                Invoke(nameof(RestoreColor), flashDuration);
            }
        }
        
        private void RestoreColor()
        {
            if (spriteRenderer != null && !isDead)
            {
                spriteRenderer.color = originalColor;
            }
        }
        
        private void UpdateVisualEffects()
        {
            // Efecto de pulso de corrupción
            if (enemyMaterial != null && !isDead)
            {
                float pulse = (Mathf.Sin(Time.time * corruptionPulseSpeed) + 1f) * 0.5f;
                float glow = 0.5f + pulse * corruptionGlowIntensity;
                
                // TODO: Actualizar propiedad de shader si existe
                // enemyMaterial.SetFloat("_GlowIntensity", glow);
            }
            
            // Indicador de vida visual
            // Podría cambiar tamaño o color según salud
            UpdateHealthVisual();
        }
        
        private void UpdateHealthVisual()
        {
            // Feedback de salud por color, no por escala (la escala la controla el editor)
            if (spriteRenderer != null && !isDead)
            {
                float t = HealthPercentage;
                spriteRenderer.color = Color.Lerp(new Color(1f, 0.2f, 0.2f), originalColor, t);
            }
        }
        
        // === MÉTODOS DE PATRULLA ===
        
        private void CreateDefaultPatrolPoints()
        {
            patrolPoints = new Transform[2];
            
            // Crear puntos de patrulla por defecto
            for (int i = 0; i < 2; i++)
            {
                GameObject point = new GameObject($"PatrolPoint_{i}");
                point.transform.SetParent(transform.parent);
                
                float xOffset = i == 0 ? -patrolDistance : patrolDistance;
                point.transform.position = startPosition + new Vector2(xOffset, 0);
                
                patrolPoints[i] = point.transform;
            }
        }
        
        private void StartPatrolling()
        {
            isPatrolling = true;
            currentPatrolIndex = 0;
        }
        
        private int GetNextPatrolIndex()
        {
            if (currentPatrolIndex == patrolPoints.Length - 1)
            {
                if (loopPatrol)
                {
                    return 0;
                }
                else
                {
                    // Invertir dirección
                    System.Array.Reverse(patrolPoints);
                    return 0;
                }
            }
            
            return currentPatrolIndex + 1;
        }
        
        // === MÉTODOS PÚBLICOS ===
        
        public void SetEnemyType(EnemyType type)
        {
            enemyType = type;
            ConfigureForType();
        }
        
        public void SetHealth(int health)
        {
            maxHealth = health;
            currentHealth = health;
        }
        
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }
        
        public void SetAttackRange(float range)
        {
            attackRange = range;
        }
        
        public void SetAggro(bool aggro)
        {
            isAggroed = aggro;
        }
        
        public void Heal(int amount)
        {
            if (isDead) return;
            
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged.Invoke(currentHealth);
        }
        
        // === MÉTODOS DE DEPURACIÓN ===
        
        private void OnDrawGizmos()
        {
            // Rango de ataque
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Zona de patrulla
            if (enemyType == EnemyType.TypeB_Patrol && patrolDistance > 0)
            {
                Gizmos.color = Color.yellow;
                Vector3 leftPoint = (Vector3)startPosition + Vector3.left * patrolDistance;
                Vector3 rightPoint = (Vector3)startPosition + Vector3.right * patrolDistance;
                Gizmos.DrawLine(leftPoint, rightPoint);
                Gizmos.DrawWireSphere(leftPoint, 0.3f);
                Gizmos.DrawWireSphere(rightPoint, 0.3f);
            }
            
            // Salud visual (para depuración)
            Gizmos.color = Color.Lerp(Color.red, Color.green, HealthPercentage);
            Gizmos.DrawIcon(transform.position + Vector3.up * 1f, "health_icon.png");
        }
        
        private void OnGUI()
        {
            if (Application.isEditor && !isDead)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
                
                if (screenPos.z > 0)
                {
                    Rect rect = new Rect(screenPos.x - 50, Screen.height - screenPos.y - 100, 100, 40);
                    GUI.Box(rect, $"<b>{enemyType}</b>\nSalud: {currentHealth}/{maxHealth}");
                }
            }
        }
    }
}