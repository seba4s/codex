using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using CODEX.Player;

namespace CODEX.Systems
{
    /// <summary>
    /// Terminal de reparación interactivo.
    /// - Detección: OnTriggerStay2D (funciona aunque el jugador spawne dentro del trigger).
    /// - Input: InputAction.started — evento puntual, no polling.
    /// - Prompt: TextMeshProUGUI en Canvas (screen-space, independiente de escala mundo).
    /// </summary>
    public class RepairTerminal : MonoBehaviour
    {
        [Header("Terminal")]
        [SerializeField] private bool singleUse = true;

        [Header("Prompt")]
        [SerializeField] private string promptText = "[E]  Reparar terminal";

        [Header("Events")]
        public UnityEvent OnActivated;

        private bool        activated;
        private bool        playerInRange;
        private GameObject  promptGO;
        private InputAction interactAction;

        // ── Awake: registrar input ────────────────────────────────────────────────

        private void Awake()
        {
            interactAction = new InputAction("Interact", InputActionType.Button);
            interactAction.AddBinding("<Keyboard>/e");
            interactAction.AddBinding("<Gamepad>/buttonWest");
            interactAction.started += OnInteractPressed;
            interactAction.Enable();
        }

        private void Start()
        {
            // Asegurar trigger
            var col = GetComponent<BoxCollider2D>();
            if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            // Prompt en Canvas (screen-space: independiente de escala mundo)
            CreateCanvasPrompt();
        }

        private void OnDestroy()
        {
            if (interactAction != null)
            {
                interactAction.started -= OnInteractPressed;
                interactAction.Disable();
                interactAction.Dispose();
            }
        }

        // ── Input ────────────────────────────────────────────────────────────────

        private void OnInteractPressed(InputAction.CallbackContext ctx)
        {
            if (playerInRange && !activated)
                Activate();
        }

        // ── Física ───────────────────────────────────────────────────────────────
        // OnTriggerStay2D: se dispara cada FixedUpdate mientras haya solapamiento.
        // Esto resuelve el caso donde el jugador ya está dentro del trigger al inicio.

        private void OnTriggerStay2D(Collider2D other)
        {
            if (activated || playerInRange) return;
            if (!IsPlayer(other)) return;
            playerInRange = true;
            ShowPrompt(true);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activated || !IsPlayer(other)) return;
            playerInRange = true;
            ShowPrompt(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsPlayer(other)) return;
            playerInRange = false;
            ShowPrompt(false);
        }

        // Busca en el root completo — el collider puede estar en un hijo del jugador
        private static bool IsPlayer(Collider2D col)
        {
            var root = col.transform.root;
            return root.CompareTag("Player")
                || root.GetComponentInChildren<PlayerController>() != null;
        }

        // ── Activar ──────────────────────────────────────────────────────────────

        private void Activate()
        {
            if (singleUse) activated = true;
            ShowPrompt(false);
            OnActivated?.Invoke();
            CODEX.Tutorial.TutorialManager.Instance?.ActivateTerminal();
        }

        public void ResetTerminal()
        {
            activated     = false;
            playerInRange = false;
            ShowPrompt(false);
        }

        // ── Prompt en Canvas (screen-space) ──────────────────────────────────────

        private void CreateCanvasPrompt()
        {
            // Buscar el Canvas de la escena
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            promptGO = new GameObject($"TerminalPrompt_{name}");
            promptGO.transform.SetParent(canvas.transform, false);

            // Panel centrado en la parte baja de la pantalla
            var rt         = promptGO.AddComponent<RectTransform>();
            rt.anchorMin   = new Vector2(0.25f, 0.03f);
            rt.anchorMax   = new Vector2(0.75f, 0.11f);
            rt.offsetMin   = rt.offsetMax = Vector2.zero;

            var bg         = promptGO.AddComponent<Image>();
            bg.color       = new Vector4(0.02f, 0.05f, 0.12f, 0.88f);

            // Texto
            var textGO     = new GameObject("Label");
            textGO.transform.SetParent(promptGO.transform, false);
            var textRT     = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = textRT.offsetMax = Vector2.zero;

            var tmp        = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text       = promptText;
            tmp.fontSize   = 28f;
            tmp.alignment  = TextAlignmentOptions.Center;
            tmp.color      = new Color(0.35f, 0.95f, 0.85f);
            tmp.fontStyle  = FontStyles.Bold;

            promptGO.SetActive(false);
        }

        private void ShowPrompt(bool visible)
        {
            if (promptGO != null) promptGO.SetActive(visible);
        }
    }
}
