using UnityEngine;
using CODEX.Player;                                            // BREAKING: era CODEX.Systems (HealthSystem eliminado)

namespace CODEX.Tutorial
{
    /// <summary>
    /// Gestiona checkpoints locales dentro de una escena de tutorial.
    /// Al morir, el jugador reaparece en el último checkpoint con 3 segmentos de vida.
    /// No hay conteo de vidas — el tutorial es espacio seguro.
    /// </summary>
    public class CheckpointManager : MonoBehaviour
    {
        public static CheckpointManager Instance { get; private set; }

        [SerializeField] private Transform defaultSpawn;
        private Transform activeCheckpoint;

        private LumaGuide luma;
        private PlayerHealth playerHealth;                     // REFACTOR: era HealthSystem

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            activeCheckpoint = defaultSpawn != null ? defaultSpawn : transform;
            luma = FindAnyObjectByType<LumaGuide>();

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.OnDied.AddListener(HandlePlayerDeath); // FIX: era HealthSystem.OnDeath (System.Action)
                    playerHealth.SetCheckpoint(activeCheckpoint);       // FIX: sincronizar checkpoint inicial
                }
            }
        }

        private void OnDestroy()                                        // FIX: faltaba — memory leak corregido
        {
            if (playerHealth != null)
                playerHealth.OnDied.RemoveListener(HandlePlayerDeath);
        }

        public void SetCheckpoint(Transform cp)
        {
            activeCheckpoint = cp;
            playerHealth?.SetCheckpoint(cp);                            // FIX: sincronizar checkpoint en PlayerHealth
        }

        private void HandlePlayerDeath()
        {
            // FIX: teleporte y restauración de HP manejados por PlayerHealth.Respawn()
            luma?.Say("El sistema te restauró. Sigue. No cuenta.");
        }
    }
}
