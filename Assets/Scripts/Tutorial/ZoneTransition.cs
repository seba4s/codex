using UnityEngine;
using UnityEngine.Events;
using CODEX.Player;
using CODEX.Systems;

namespace CODEX.Tutorial
{
    [RequireComponent(typeof(Collider2D))]
    public class ZoneTransition : MonoBehaviour
    {
        [Header("Destino")]
        [SerializeField] private Transform playerSpawnPoint;

        [Header("Fondos")]
        [SerializeField] private GameObject[] objectsToDisable;
        [SerializeField] private GameObject[] objectsToEnable;

        [Header("Evento")]
        public UnityEvent OnTransition;

        private bool used;

        private void Start()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (used || !other.CompareTag("Player")) return;
            used = true;

            // Mover jugador al inicio del siguiente bloque
            // B3 FIX: playerSpawnPoint no asignado en Inspector → NullRef al acceder .position
            if (playerSpawnPoint == null)
            {
                Debug.LogError("[ZoneTransition] playerSpawnPoint no asignado. " +
                               "Asígnalo en el Inspector.", this);
                used = false; // permitir reintento si se configura en runtime
                return;
            }

            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
                pc.TeleportTo(playerSpawnPoint.position);
            else
                other.transform.position = playerSpawnPoint.position;

            // Snap de cámara (sin slide)
            // B3 FIX: Camera.main puede ser null si la cámara no tiene tag "MainCamera"
            //         o si se destruyó durante la transición — null check obligatorio.
            if (Camera.main != null)
            {
                CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
                if (cam != null) cam.SnapToTarget();
            }
            else
            {
                Debug.LogWarning("[ZoneTransition] Camera.main es null — SnapToTarget omitido. " +
                                 "Verifica que la cámara tenga el tag 'MainCamera'.", this);
            }

            // Cambiar fondos
            foreach (var obj in objectsToDisable)
                if (obj != null) obj.SetActive(false);

            foreach (var obj in objectsToEnable)
                if (obj != null) obj.SetActive(true);

            OnTransition?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            if (playerSpawnPoint == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(playerSpawnPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, playerSpawnPoint.position);
        }
    }
}
