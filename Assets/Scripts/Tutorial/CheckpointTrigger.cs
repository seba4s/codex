using UnityEngine;
using UnityEngine.Events;

namespace CODEX.Tutorial
{
    /// <summary>
    /// Coloca este trigger en el punto donde el jugador activa un checkpoint.
    /// Al entrar, registra la posición en el CheckpointManager.
    /// OnPlayerPassed: UnityEvent wirable en Inspector — usado por Block06 para saber que el abismo fue cruzado.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;

        [Header("Evento")]
        public UnityEvent OnPlayerPassed;   // C3 FIX: antes faltaba este evento; T06_SceneSetup lo conecta a Block06.OnSequenceCleared

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
            OnPlayerPassed?.Invoke();
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
