using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace CODEX.Tutorial
{
    /// <summary>
    /// LUMA: guia narrativa del tutorial. Muestra dialogo en cola, detecta inactividad
    /// y da empujes suaves al jugador cuando lleva demasiado tiempo sin actuar.
    /// </summary>
    public class LumaGuide : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float charDelay = 0.03f;

        [Header("Idle Nudge")]
        [SerializeField] private float idleNudgeDelay = 6f;
        [SerializeField] private string idleNudgeMessage = "...¿Vas a quedarte ahí todo el día?";

        [Header("Efecto flotante")]
        [SerializeField] private float floatAmplitude = 0.15f;
        [SerializeField] private float floatSpeed = 1.5f;

        private Queue<(string speaker, string message)> dialogueQueue = new();
        private bool isTalking;
        private Vector3 startPos;
        private float idleTimer;
        private bool idleNudgeEnabled;
        private bool nudgeFired;

        private void Awake()
        {
            startPos = transform.localPosition;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        private void Update()
        {
            // Efecto de flotar
            transform.localPosition = startPos + Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

            // Detector de inactividad
            if (idleNudgeEnabled && !nudgeFired && !isTalking)
            {
                idleTimer += Time.deltaTime;
                if (idleTimer >= idleNudgeDelay)
                {
                    nudgeFired = true;
                    Say("LUMA", idleNudgeMessage);
                }
            }
        }

        public void Say(string speaker, string message)
        {
            dialogueQueue.Enqueue((speaker, message));
            if (!isTalking)
                StartCoroutine(DrainQueue());
        }

        public void Say(string message) => Say("LUMA", message);

        public void SayEraserOmega(string message) => Say("ERASER-Omega", message);

        public void EnableIdleNudge(string customMessage = null)
        {
            if (customMessage != null) idleNudgeMessage = customMessage;
            idleTimer = 0f;
            nudgeFired = false;
            idleNudgeEnabled = true;
        }

        public void DisableIdleNudge()
        {
            idleNudgeEnabled = false;
            idleTimer = 0f;
        }

        public void ResetNudge()
        {
            nudgeFired = false;
            idleTimer = 0f;
        }

        private IEnumerator DrainQueue()
        {
            isTalking = true;

            while (dialogueQueue.Count > 0)
            {
                var (speaker, msg) = dialogueQueue.Dequeue();
                yield return ShowLine(speaker, msg);
                yield return new WaitForSeconds(0.4f);
            }

            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            isTalking = false;
        }

        private IEnumerator ShowLine(string speaker, string message)
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            if (speakerText != null) speakerText.text = speaker;
            if (messageText != null) messageText.text = "";

            // Typewriter
            foreach (char c in message)
            {
                if (messageText != null) messageText.text += c;
                yield return new WaitForSeconds(charDelay);
            }

            // Tiempo de lectura proporcional al largo del mensaje
            float readTime = Mathf.Clamp(message.Length * 0.04f, 2f, 6f);
            yield return new WaitForSeconds(readTime);
        }

        public bool IsTalking => isTalking;
    }
}
