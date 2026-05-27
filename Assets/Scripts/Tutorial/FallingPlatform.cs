using System.Collections;
using UnityEngine;
using CODEX.Player;

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
            // Suscribir en Awake, no en OnEnable:
            // ResetRoutine llama SetActive(false), lo que dispararía OnDisable y
            // desuscribiría la plataforma antes de que el jugador respawnee.
            PlayerHealth.OnPlayerRespawned += ResetPlatform;
        }

        private void Start()
        {
            // Ignorar colisión entre todas las FallingPlatform de la escena.
            // Se hace una sola vez al inicio — no hay que repetirlo en cada caída.
            var myCol = GetComponent<Collider2D>();
            var allPlatforms = FindObjectsByType<FallingPlatform>(FindObjectsInactive.Include);
            foreach (var other in allPlatforms)
            {
                if (other == this) continue;
                var otherCol = other.GetComponent<Collider2D>();
                if (otherCol != null)
                    Physics2D.IgnoreCollision(myCol, otherCol, true);
            }
        }

        private void OnDestroy()
        {
            PlayerHealth.OnPlayerRespawned -= ResetPlatform;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (isFalling || isResetting) return;
            // Buscar en el root — el collider puede estar en un hijo sin tag "Player"
            if (!col.transform.root.CompareTag("Player")) return;

            // Solo cae si el jugador está ENCIMA de la plataforma.
            // Usamos posición Y en lugar de contact.normal para evitar ambigüedad:
            // en Unity 2D, la dirección del normal depende de cuál cuerpo es kinematic/dynamic
            // y cambia entre versiones. Comparar Y es inequívoco.
            if (col.transform.root.position.y > transform.position.y)
            {
                StartCoroutine(FallRoutine());
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
