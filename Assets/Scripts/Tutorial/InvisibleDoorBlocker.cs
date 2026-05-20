using UnityEngine;
using CODEX.Enemies;

namespace CODEX.Tutorial
{
    /// <summary>
    /// Collider invisible que bloquea el paso hasta que el enemigo designado muere.
    /// Cuando el jugador choca, LUMA dice que debe eliminar al enemigo primero.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class InvisibleDoorBlocker : MonoBehaviour
    {
        [SerializeField] private InfectedFile targetEnemy;
        [SerializeField] private LumaGuide luma;
        [SerializeField] private string blockedMessage = "Debes eliminar al enemigo primero para poder avanzar.";

        private Collider2D col;
        private bool messageShown;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            col.isTrigger = false;
        }

        private void Start()
        {
            if (targetEnemy != null)
                targetEnemy.OnEnemyDeath.AddListener(_ => Open());
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            if (messageShown) return;

            messageShown = true;

            if (luma != null)
                luma.Say("LUMA", blockedMessage);
        }

        private void Open()
        {
            col.enabled = false;
            gameObject.SetActive(false);
        }

        public void SetTarget(InfectedFile enemy) => targetEnemy = enemy;
        public void SetLuma(LumaGuide guide) => luma = guide;
    }
}
