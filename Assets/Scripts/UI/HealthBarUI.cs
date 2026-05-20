using UnityEngine;
using UnityEngine.UI;
using CODEX.Systems;

namespace CODEX.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;

        private HealthSystem health;

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            health = player.GetComponent<HealthSystem>();
            if (health == null) return;

            health.OnHealthChanged += OnHealthChanged;
            OnHealthChanged(health.CurrentHealth, health.MaxHealth);
        }

        private void OnDestroy()
        {
            if (health != null)
                health.OnHealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(int current, int max)
        {
            if (fillImage == null) return;
            fillImage.fillAmount = max > 0 ? (float)current / max : 0f;
        }
    }
}
