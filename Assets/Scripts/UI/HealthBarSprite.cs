using UnityEngine;
using UnityEngine.UI;
using CODEX.Player;

namespace CODEX.UI
{
    /// <summary>
    /// Barra de salud persistente entre escenas.
    /// Mantiene el sprite que el usuario asignó al Image y controla
    /// únicamente fillAmount para que la barra se vacíe horizontalmente.
    /// No crea ningún hijo ni modifica el sprite.
    /// </summary>
    public class HealthBarSprite : MonoBehaviour
    {
        private Image        bar;
        private PlayerHealth health;

        // ── Lifecycle ────────────────────────────────────────────────────────────

        private void Start()
        {
            if (GetComponentInParent<Canvas>() == null)
            {
                Debug.LogError("[HealthBarSprite] Debe estar dentro de un Canvas.", this);
                enabled = false;
                return;
            }

            bar = GetComponent<Image>();
            if (bar == null)
            {
                Debug.LogError("[HealthBarSprite] Falta el componente Image en " + name, this);
                enabled = false;
                return;
            }

            // Activar modo fill sin tocar el sprite ni el color del usuario
            bar.type       = Image.Type.Filled;
            bar.fillMethod = Image.FillMethod.Horizontal;
            bar.fillOrigin = 0;   // de izquierda a derecha
            bar.fillAmount = 1f;

            InvokeRepeating(nameof(TryFindHealth), 0f, 0.2f);
        }

        private void OnDestroy()
        {
            if (health != null)
                health.OnHealthChanged -= OnHealthChanged;
        }

        // ── Búsqueda de PlayerHealth ─────────────────────────────────────────────

        private void TryFindHealth()
        {
            if (health != null) { CancelInvoke(nameof(TryFindHealth)); return; }

            var go = GameObject.FindGameObjectWithTag("Player");
            if (go == null) return;

            health = go.GetComponent<PlayerHealth>()
                  ?? go.GetComponentInChildren<PlayerHealth>()
                  ?? FindAnyObjectByType<PlayerHealth>();

            if (health == null) return;

            CancelInvoke(nameof(TryFindHealth));
            health.OnHealthChanged += OnHealthChanged;
            OnHealthChanged(health.CurrentHP, health.MaxHP);
        }

        // ── Actualizar fill ──────────────────────────────────────────────────────

        private void OnHealthChanged(int current, int max)
        {
            if (bar == null || max <= 0) return;
            bar.fillAmount = Mathf.Clamp01((float)current / max);
        }
    }
}
