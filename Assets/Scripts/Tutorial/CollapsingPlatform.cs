using System.Collections;
using UnityEngine;

namespace CODEX.Tutorial
{
    /// <summary>
    /// Bloque 6 – Plataformas que colapsan cuando el jugador se para encima.
    /// Vibra → grietas visibles → cae. Se restaura al reaparecer del checkpoint.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class CollapsingPlatform : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private float timeBeforeCollapse = 3f;
        [SerializeField] private float shakeMagnitude = 0.05f;
        [SerializeField] private float restoreDelay = 3f;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer crackOverlay;  // sprite de grietas (opcional)

        private Rigidbody2D rb;
        private Collider2D col;
        private Vector3 originalPosition;
#pragma warning disable CS0414
        private bool isShaking;
#pragma warning restore CS0414
        private bool hasCollapsed;
        private Coroutine collapseRoutine;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            originalPosition = transform.position;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (hasCollapsed || !other.gameObject.CompareTag("Player")) return;
            if (collapseRoutine == null)
                collapseRoutine = StartCoroutine(CollapseSequence());
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            // Si el jugador sale antes de que colapse, se detiene el shake
            // pero el colapso ya está en marcha — por diseño no se cancela
        }

        private IEnumerator CollapseSequence()
        {
            isShaking = true;
            float elapsed = 0f;

            while (elapsed < timeBeforeCollapse)
            {
                elapsed += Time.deltaTime;

                // Vibrar
                float progress = elapsed / timeBeforeCollapse;
                Vector3 shake = (Vector3)Random.insideUnitCircle * shakeMagnitude * progress;
                transform.position = originalPosition + shake;

                // Mostrar grietas progresivamente
                if (crackOverlay != null)
                    crackOverlay.color = new Color(1f, 1f, 1f, progress);

                yield return null;
            }

            isShaking = false;
            hasCollapsed = true;
            transform.position = originalPosition;

            // Caer
            rb.bodyType = RigidbodyType2D.Dynamic;
            col.enabled = false;

            yield return new WaitForSeconds(restoreDelay);
            Restore();
        }

        private void Restore()
        {
            hasCollapsed = false;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            transform.position = originalPosition;
            col.enabled = true;
            collapseRoutine = null;
            if (crackOverlay != null)
                crackOverlay.color = new Color(1f, 1f, 1f, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>()?.bounds.size ?? Vector3.one);
        }
    }
}
