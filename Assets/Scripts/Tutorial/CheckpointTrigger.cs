using UnityEngine;

namespace CODEX.Tutorial
{
    /// <summary>
    /// Coloca este trigger en el punto donde el jugador activa un checkpoint.
    /// Al entrar, registra la posición en el CheckpointManager.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        private bool activated;

        private void Start()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activated || !other.CompareTag("Player")) return;
            activated = true;
            CheckpointManager.Instance?.SetCheckpoint(spawnPoint != null ? spawnPoint : transform);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            if (spawnPoint != null)
                Gizmos.DrawSphere(spawnPoint.position, 0.25f);
        }
    }
}
