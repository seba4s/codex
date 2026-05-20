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

        private LumaGuide luma;
        private bool used;

        private void Start()
        {
            GetComponent<Collider2D>().isTrigger = true;
            luma = FindAnyObjectByType<LumaGuide>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (used || !other.CompareTag("Player")) return;

            if (requireCondition && !ConditionMet())
            {
                luma?.Say(conditionNotMetMessage);
                return;
            }

            used = true;

            if (TutorialManager.Instance != null)
                TutorialManager.Instance.LoadNextBlock();
        }

        protected virtual bool ConditionMet() => true;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}
