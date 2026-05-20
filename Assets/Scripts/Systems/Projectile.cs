using UnityEngine;

namespace CODEX.Systems
{
    public class Projectile : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private int damage = 1;
        [SerializeField] private float speed = 25f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private LayerMask obstacleLayers;
        [SerializeField] private GameObject hitEffectPrefab;
        
        [Header("Estado")]
        private Vector2 direction;
        private float lifeTimer;
        private bool hasHit;
        private Rigidbody2D rb;
        
        public int Damage => damage;
        public Vector2 Direction => direction;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        public void Initialize(Vector2 shootDirection, float projectileSpeed, int projectileDamage, 
                              LayerMask targetLayerMask, float projectileLifetime)
        {
            direction = shootDirection.normalized;
            speed = projectileSpeed;
            damage = projectileDamage;
            targetLayers = targetLayerMask;
            lifetime = projectileLifetime;
            lifeTimer = 0f;
            hasHit = false;
            
            // Configurar rotación basada en dirección
            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
        
        private void Start()
        {
            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
            }
        }
        
        private void Update()
        {
            lifeTimer += Time.deltaTime;
            
            if (lifeTimer >= lifetime && !hasHit)
            {
                DestroyProjectile();
            }
        }
        
        private void FixedUpdate()
        {
            if (!hasHit && rb != null)
            {
                rb.linearVelocity = direction * speed;
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasHit) return;
            
            // Chequear si es un obstáculo
            if ((obstacleLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                HitObstacle(other);
                return;
            }
            
            // Chequear si es un objetivo
            if ((targetLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                HitTarget(other);
                return;
            }
        }
        
        private void HitTarget(Collider2D target)
        {
            hasHit = true;
            
            // Aplicar daño
            // HealthSystem targetHealth = target.GetComponent<HealthSystem>();
            // if (targetHealth != null)
            // {
            //     targetHealth.TakeDamage(damage, transform.position);
            // }
            
            // También podríamos tener otros componentes que reciban daño
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, transform.position);
            }
            
            // Efecto de impacto
            SpawnHitEffect();
            
            // Destruir proyectil
            DestroyProjectile();
        }
        
        private void HitObstacle(Collider2D obstacle)
        {
            hasHit = true;
            
            // Efecto de impacto
            SpawnHitEffect();
            
            // Destruir proyectil
            DestroyProjectile();
        }
        
        private void SpawnHitEffect()
        {
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        
        private void DestroyProjectile()
        {
            // Podríamos añadir una animación de desaparición
            Destroy(gameObject);
        }
        
        // === MÉTODOS DE CONFIGURACIÓN ===
        public void SetDamage(int newDamage)
        {
            damage = newDamage;
        }
        
        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
            if (rb != null && !hasHit)
            {
                rb.linearVelocity = direction * speed;
            }
        }
        
        public void SetDirection(Vector2 newDirection)
        {
            direction = newDirection.normalized;
            if (rb != null && !hasHit)
            {
                rb.linearVelocity = direction * speed;
            }
        }
        
        public void SetTargetLayers(LayerMask newTargetLayers)
        {
            targetLayers = newTargetLayers;
        }
        
        // === INTERFAZ PARA DAÑO ===
        public interface IDamageable
        {
            void TakeDamage(int damage, Vector2 sourcePosition);
        }
    }
}