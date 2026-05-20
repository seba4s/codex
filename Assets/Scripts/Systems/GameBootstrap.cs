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

        public static void ApplyResolution()
        {
            Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        }
    }
}
