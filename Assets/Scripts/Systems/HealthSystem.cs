using UnityEngine;
using System.Collections;

namespace CODEX.Systems
{
    /// <summary>
    /// Sistema de salud del jugador. 5 segmentos. Las gafas de CODIGO-7 cambian
    /// de color: azul (100%) → amarillo (60%) → rojo (20%).
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private int maxHealth = 5;
        [SerializeField] private float invincibilityAfterHit = 1.2f;

        [Header("Visual - Gafas")]
        [SerializeField] private SpriteRenderer gogglesRenderer;
        private static readonly Color ColorFull    = new Color(0.2f, 0.6f, 1f);   // azul
        private static readonly Color ColorMid     = new Color(1f, 0.85f, 0.1f);  // amarillo
        private static readonly Color ColorLow     = new Color(1f, 0.2f, 0.2f);   // rojo

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public float HealthPercent => (float)CurrentHealth / maxHealth;
        public bool IsAlive => CurrentHealth > 0;

        private bool isInvincible;

        // Eventos
        public System.Action<int, int> OnHealthChanged;   // (current, max)
        public System.Action OnDamaged;
        public System.Action OnDeath;
        public System.Action OnRevived;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (isInvincible || !IsAlive) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            OnDamaged?.Invoke();
            UpdateGogglesColor();

            if (CurrentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
            else
            {
                StartCoroutine(InvincibilityFrames());
            }
        }

        public void Heal(int amount)
        {
            if (!IsAlive) return;
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            UpdateGogglesColor();
        }

        public void Revive(int withHealth = -1)
        {
            CurrentHealth = withHealth > 0 ? Mathf.Min(withHealth, maxHealth) : maxHealth;
            isInvincible = false;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            UpdateGogglesColor();
            OnRevived?.Invoke();
        }

        private IEnumerator InvincibilityFrames()
        {
            isInvincible = true;
            yield return new WaitForSeconds(invincibilityAfterHit);
            isInvincible = false;
        }

        private void UpdateGogglesColor()
        {
            if (gogglesRenderer == null) return;
            if (HealthPercent > 0.6f)      gogglesRenderer.color = ColorFull;
            else if (HealthPercent > 0.2f) gogglesRenderer.color = ColorMid;
            else                           gogglesRenderer.color = ColorLow;
        }

        public bool IsInvincible => isInvincible;
    }
}
