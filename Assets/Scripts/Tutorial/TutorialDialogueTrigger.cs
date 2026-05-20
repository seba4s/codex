using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace CODEX.Tutorial
{
    [RequireComponent(typeof(Collider2D))]
    public class TutorialDialogueTrigger : MonoBehaviour
    {
        [Header("Mensaje")]
        [TextArea(2, 5)]
        [SerializeField] private string message = "Nuevo mensaje tutorial";
        [SerializeField] private float displayDuration = 4f;
        [SerializeField] private bool triggerOnce = true;

        [Header("UI")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI dialogueText;

        [Header("Evento")]
        public UnityEvent OnTriggered;

        private bool hasTriggered;

        private void Start()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (triggerOnce && hasTriggered) return;

            hasTriggered = true;
            OnTriggered?.Invoke();
            StartCoroutine(ShowMessage());
        }

        private IEnumerator ShowMessage()
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            if (dialogueText != null) dialogueText.text = message;

            yield return new WaitForSeconds(displayDuration);

            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        public void ResetTrigger() => hasTriggered = false;
    }
}
