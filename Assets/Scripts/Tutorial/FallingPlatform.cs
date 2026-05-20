using System.Collections;
using UnityEngine;

namespace CODEX.Tutorial
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class FallingPlatform : MonoBehaviour
    {
        [Header("Caída")]
        [SerializeField] private float fallDelay = 0.5f;
        [SerializeField] private float resetDelay = 3f;

        private Rigidbody2D rb;
        private Vector3 startPosition;
        private bool isFalling;
        private bool isResetting;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            startPosition = transform.position;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (isFalling || isResetting) return;
            if (!col.gameObject.CompareTag("Player")) return;

            // Solo cae si el jugador aterriza encima
            foreach (ContactPoint2D contact in col.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    StartCoroutine(FallRoutine());
                    break;
                }
            }
        }

        private IEnumerator FallRoutine()
        {
            isFalling = true;
            yield return new WaitForSeconds(fallDelay);

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f;

            yield return new WaitForSeconds(resetDelay);

            StartCoroutine(ResetRoutine());
        }

        private IEnumerator ResetRoutine()
        {
            isResetting = true;
            gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            transform.position = startPosition;
            gameObject.SetActive(true);

            isFalling = false;
            isResetting = false;
        }

        public void ResetPlatform()
        {
            StopAllCoroutines();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            transform.position = startPosition;
            isFalling = false;
            isResetting = false;
            gameObject.SetActive(true);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>().bounds.size);
        }
    }
}
