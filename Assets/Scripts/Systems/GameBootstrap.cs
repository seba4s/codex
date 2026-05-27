using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CODEX.Systems
{
    public class GameBootstrap : MonoBehaviour
    {
        private static GameBootstrap _instance;

        private void Awake()
        {
            if (_instance != null) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            StartCoroutine(ApplyAfterFrame());
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(ApplyAfterFrame());
        }

        private IEnumerator ApplyAfterFrame()
        {
            yield return null; // esperar un frame
            ApplyResolution();
        }

        // D3 FIX (F4): resolución leída desde PlayerPrefs en vez de hardcodeada 1920×1080.
        // Usa las mismas keys que OptionsController para coherencia:
        //   "IndiceResolucion" (int) — índice en Screen.resolutions[]
        //   "PantallaCompleta" (int) — 1 = fullscreen, 0 = windowed
        // Default: última resolución nativa del monitor, pantalla completa.
        public static void ApplyResolution()
        {
            bool fullscreen = PlayerPrefs.GetInt("PantallaCompleta", 1) == 1;
            FullScreenMode mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

            Resolution[] resolutions = Screen.resolutions;
            if (resolutions.Length == 0)
            {
                // Fallback absoluto si el sistema no reporta resoluciones
                Screen.SetResolution(1920, 1080, mode);
                return;
            }

            // Índice guardado, con clamp defensivo por si el monitor cambió
            int idx = PlayerPrefs.GetInt("IndiceResolucion", resolutions.Length - 1);
            idx = Mathf.Clamp(idx, 0, resolutions.Length - 1);

            Resolution res = resolutions[idx];
            Screen.SetResolution(res.width, res.height, mode);
        }
    }
}
