using System.Collections;
using UnityEngine;
using TMPro;

namespace CODEX.Tutorial
{
    /// <summary>
    /// Bloque 1 — Pantalla de diagnóstico de apertura.
    /// Muestra texto verde sobre negro antes de que aparezca CODIGO-7.
    /// </summary>
    public class DiagnosticBootScreen : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject screenRoot;
        [SerializeField] private TextMeshProUGUI bootText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Configuración")]
        [SerializeField] private float lineDelay = 0.6f;
        [SerializeField] private float fadeOutDuration = 1f;

        private static readonly string[] BootLines =
        {
            "> SISTEMA OPERATIVO: CARGANDO...",
            "> AMENAZA DETECTADA EN SECTOR: ALMACENAMIENTO",
            "> ACTIVANDO PROTOCOLO DE INTERVENCIÓN...",
            "> CODIGO-7: ONLINE"
        };

        public System.Action OnBootComplete;

        private void Start()
        {
            if (screenRoot != null) screenRoot.SetActive(true);
            StartCoroutine(RunBootSequence());
        }

        private IEnumerator RunBootSequence()
        {
            if (bootText != null) bootText.text = "";

            foreach (string line in BootLines)
            {
                yield return new WaitForSeconds(lineDelay);
                if (bootText != null) bootText.text += line + "\n";
            }

            yield return new WaitForSeconds(1.2f);
            yield return FadeOut();

            if (screenRoot != null) screenRoot.SetActive(false);
            OnBootComplete?.Invoke();
        }

        private IEnumerator FadeOut()
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                canvasGroup.alpha = 1f - (elapsed / fadeOutDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
    }
}
