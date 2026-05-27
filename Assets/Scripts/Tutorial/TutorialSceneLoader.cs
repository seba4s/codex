using UnityEngine;

namespace CODEX.Tutorial
{
    /// <summary>
    /// Trigger que carga la siguiente escena del tutorial al entrar el jugador.
    /// Coloca este componente en el collider del portal / borde derecho de la escena.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TutorialSceneLoader : MonoBehaviour
    {
        [Header("Opciones")]
        [SerializeField] private bool requireCondition = false;
        [SerializeField] private string conditionNotMetMessage = "Completa el objetivo primero.";

        [Header("Condición de enemigos (opcional)")]
        // D2 FIX (F3): si se asigna, ConditionMet() espera a que todos los enemigos estén derrotados.
        // Uso: requireCondition = true + asignar este campo en Inspector.
        // Si es null, ConditionMet() devuelve true (comportamiento base sin restricción).
        [SerializeField] private TutorialEnemyCounter enemyCondition;

        private LumaGuide luma;
        private bool used;

        private void Start()
        {
            GetComponent<Collider2D>().isTrigger = true;
            luma = FindAnyObjectByType<LumaGuide>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (used || !other.transform.root.CompareTag("Player")) return;

            if (requireCondition && !ConditionMet())
            {
                luma?.Say(conditionNotMetMessage);
                return;
            }

            used = true;

            if (TutorialManager.Instance != null)
                TutorialManager.Instance.LoadNextBlock();
        }

        // D2 FIX (F3): implementación concreta en vez de siempre-true.
        // Patrón: template method — subclases pueden override para condiciones personalizadas.
        // Sin enemyCondition asignado → true (sin restricción, comportamiento original).
        // Con enemyCondition asignado → true solo si TutorialEnemyCounter.AllDefeated.
        protected virtual bool ConditionMet()
        {
            if (enemyCondition != null)
                return enemyCondition.AllDefeated;
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}
