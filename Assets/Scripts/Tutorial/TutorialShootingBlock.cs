using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace CODEX.Tutorial
{
    [RequireComponent(typeof(Collider2D))]
    public class TutorialShootingBlock : MonoBehaviour
    {
        [Header("Objetivos")]
        [SerializeField] private ShootingTarget[] targets;

        [Header("UI")]
        [SerializeField] private GameObject hintPanel;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private string hintMessage = "Apunta con el ratón\n<b>Clic</b> para disparar";

        [Header("Completado")]
        [SerializeField] private UnityEvent OnBlockCompleted;
        [SerializeField] private GameObject completionGate;

        private int targetsDestroyed;
        private bool completed;

        private void Start()
        {
            GetComponent<Collider2D>().isTrigger = true;

            foreach (var target in targets)
            {
                if (target != null)
                    target.OnDestroyed += HandleTargetDestroyed;
            }

            if (hintPanel != null) hintPanel.SetActive(false);
            UpdateProgress();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (completed || !other.CompareTag("Player")) return;
            if (hintPanel != null) hintPanel.SetActive(true);
            if (hintText != null) hintText.text = hintMessage;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (hintPanel != null) hintPanel.SetActive(false);
        }

        private void HandleTargetDestroyed()
        {
            targetsDestroyed++;
            UpdateProgress();

            if (targetsDestroyed >= targets.Length)
                CompleteBlock();
        }

        private void UpdateProgress()
        {
            if (progressText != null)
                progressText.text = $"{targetsDestroyed} / {targets.Length}";
        }

        private void CompleteBlock()
        {
            completed = true;
            if (hintPanel != null) hintPanel.SetActive(false);
            if (completionGate != null) completionGate.SetActive(false);
            OnBlockCompleted?.Invoke();
        }
    }
}
