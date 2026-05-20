using UnityEngine;
using UnityEngine.Events;

namespace CODEX.Systems
{
    public class RepairTerminal : MonoBehaviour
    {
        [Header("Terminal")]
        [SerializeField] private bool singleUse = true;
#pragma warning disable CS0414
        [SerializeField] private float interactionRadius = 1.5f;
#pragma warning restore CS0414

        [Header("Events")]
        public UnityEvent OnActivated;

        private bool activated;
        private bool playerInRange;

        private void Update()
        {
            if (!playerInRange || activated) return;
            if (Input.GetKeyDown(KeyCode.E))
                Activate();
        }

        private void Activate()
        {
            if (singleUse) activated = true;
            OnActivated?.Invoke();
            CODEX.Tutorial.TutorialManager.Instance?.ActivateTerminal();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) playerInRange = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player")) playerInRange = false;
        }

        public void ResetTerminal()
        {
            activated = false;
            playerInRange = false;
        }
    }
}
