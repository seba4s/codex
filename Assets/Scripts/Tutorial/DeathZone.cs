using UnityEngine;
using CODEX.Player;

namespace CODEX.Tutorial
{
    /// <summary>
    /// Zona de muerte: mata al jugador instantáneamente al entrar en el trigger.
    /// Coloca este script en cualquier objeto con BoxCollider2D (isTrigger = true).
    /// El jugador respawneará en el último checkpoint activado.
    ///
    /// Usa tanto OnTriggerEnter2D como OnTriggerStay2D para cubrir caídas rápidas
    /// donde el jugador puede atravesar el trigger en un solo frame.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DeathZone : MonoBehaviour
    {
        private void Awake()
        {
            // Garantizar que el collider es siempre un trigger
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)  => TryKill(other);
        private void OnTriggerStay2D(Collider2D other)   => TryKill(other);

        private void TryKill(Collider2D other)
        {
            var root = other.transform.root;

            var health = root.GetComponent<PlayerHealth>()
                      ?? root.GetComponentInChildren<PlayerHealth>();

            if (health == null || health.IsDead) return;

            health.Kill();
        }
    }
}
