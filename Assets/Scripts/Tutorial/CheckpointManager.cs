using UnityEngine;

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

            // Escuchar muerte del jugador
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var health = player.GetComponent<CODEX.Systems.HealthSystem>();
                if (health != null)
                    health.OnDeath += HandlePlayerDeath;
            }
        }

        public void SetCheckpoint(Transform cp)
        {
            activeCheckpoint = cp;
        }

        private void HandlePlayerDeath()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            // Restaurar posición
            var pc = player.GetComponent<CODEX.Player.PlayerController>();
            if (pc != null)
                pc.TeleportTo(activeCheckpoint.position);
            else
                player.transform.position = activeCheckpoint.position;

            // Restaurar salud a 3
            var health = player.GetComponent<CODEX.Systems.HealthSystem>();
            health?.Revive(3);

            luma?.Say("El sistema te restauró. Sigue. No cuenta.");
        }
    }
}
