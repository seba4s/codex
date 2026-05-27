using System.Collections;
using UnityEngine;
using TMPro;

namespace CODEX.Tutorial
{
    /// <summary>
    /// Bloque 8 — El primer momento emocional del juego.
    /// Al activar el tercer terminal, aparece el Registro de Arranque de Sebastian
    /// con el diálogo final antes del Puerto de Salida.
    /// </summary>
    public class FirstFileKeyEvent : MonoBehaviour
    {
        [Header("UI del archivo")]
        [SerializeField] private GameObject filePanel;
        [SerializeField] private TextMeshProUGUI fileContent;
        [SerializeField] private CanvasGroup filePanelGroup;

        [Header("Referencias")]
        [SerializeField] private LumaGuide luma;
        [SerializeField] private GameObject exitPort;         // Puerto USB que se ilumina al final
        [SerializeField] private CODEX.Player.PlayerController player;

        [Header("Configuración")]
        [SerializeField] private float pauseBeforeReveal = 1.5f;

        [TextArea(3, 8)]
        [SerializeField] private string fileText =
            "REGISTRO_ARRANQUE_001.txt\n" +
            "---\n" +
            "Primer proyecto guardado. No sé si esto va a funcionar,\n" +
            "pero lo intentamos.\n" +
            "— S";

        private void Start()
        {
            if (filePanel != null) filePanel.SetActive(false);
            if (exitPort != null) exitPort.SetActive(false);

            // Escuchar el tercer terminal
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.OnTerminalActivated += HandleTerminalActivated;
        }

        private void OnDestroy()
        {
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.OnTerminalActivated -= HandleTerminalActivated;
        }

        private void HandleTerminalActivated(int count)
        {
            if (count >= TutorialManager.TotalTerminals)
                StartCoroutine(PlayFinalSequence());
        }

        private IEnumerator PlayFinalSequence()
        {
            // Congelar input del jugador
            player?.SetInputEnabled(false);

            yield return new WaitForSeconds(pauseBeforeReveal);

            // LUMA anuncia el puerto activo
            luma?.Say("Puerto de transferencia activo. Pero antes de que saltes... mira esto.");
            yield return new WaitForSeconds(3f);

            // Mostrar archivo
            if (filePanel != null)
            {
                filePanel.SetActive(true);
                if (fileContent != null) fileContent.text = fileText;
            }

            yield return new WaitForSeconds(4f);

            // Diálogo emocional
            luma?.Say("Eso es de Sebastian. Un registro antiguo. No es solo un dato, ¿verdad?");
            yield return new WaitForSeconds(4f);

            // CODIGO-7 responde (podría ser texto en el HUD o animación)
            luma?.Say("CODIGO-7", "...No. No lo es.");
            yield return new WaitForSeconds(3f);

            // ERASER-Omega interrumpe
            luma?.SayEraserOmega("Disfrutaron de ese pequeño recuerdo, ¿eh? Hay mucho más que borrar. Los espero en RAM, donde todo dura menos que una promesa.");
            yield return new WaitForSeconds(5f);

            // Cerrar panel y activar puerto
            if (filePanel != null) filePanel.SetActive(false);
            if (exitPort != null) exitPort.SetActive(true);

            // Restaurar input — el jugador tiene 2s para ver el puerto encenderse
            player?.SetInputEnabled(true);

            // B7 FIX (F2): el juego se quedaba suspendido aquí — no había transición al Nivel 1.
            // Opción A: auto-transición 2s después del puerto, con pixel-glitch temático.
            // IMPORTANTE: "Nivel1_DiscoDuro" debe estar en File → Build Settings.
            yield return new WaitForSeconds(2f);

            // Marcar tutorial completo en el manager global (si existe)
            TutorialManager.Instance?.OnTutorialComplete?.Invoke();

            if (SceneTransition.Instance != null)
            {
                SceneTransition.Instance.CargarEscena("Nivel1_DiscoDuro");
            }
            else
            {
                // FALLBACK: SceneTransition no en escena — carga directa
                Debug.LogWarning("[FirstFileKeyEvent] SceneTransition no encontrado — " +
                                 "carga directa sin glitch. Añade el prefab SceneTransition.");
                UnityEngine.SceneManagement.SceneManager.LoadScene("Nivel1_DiscoDuro");
            }
        }
    }
}
