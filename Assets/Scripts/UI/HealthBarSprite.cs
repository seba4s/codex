using UnityEngine;
using UnityEngine.UI;
using CODEX.Systems;

namespace CODEX.UI
{
    /// <summary>
    /// Muestra la barra de vida usando sprites del sheet ui x1.png.
    /// healthSprites[0] = vida vacía, healthSprites[last] = vida llena.
    /// </summary>
    public class HealthBarSprite : MonoBehaviour
    {
        [SerializeField] private Image displayImage;
        [SerializeField] private Sprite[] healthSprites;

        private HealthSystem health;

        private void Start()
        {
            // Mostrar sprite de vida llena por defecto (último del array)
            if (displayImage != null && healthSprites != null && healthSprites.Length > 0)
                displayImage.sprite = healthSprites[healthSprites.Length - 1];

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) { Debug.LogWarning("[HealthBarSprite] No se encontró Player."); return; }

            health = player.GetComponent<HealthSystem>();
            if (health == null) { Debug.LogWarning("[HealthBarSprite] Player no tiene HealthSystem."); return; }

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
            if (displayImage == null || healthSprites == null || healthSprites.Length == 0) return;

            float fraction = max > 0 ? (float)current / max : 0f;
            int index = Mathf.RoundToInt(fraction * (healthSprites.Length - 1));
            index = Mathf.Clamp(index, 0, healthSprites.Length - 1);
            displayImage.sprite = healthSprites[index];
        }
    }
}
