using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace CODEX.Tutorial
{
    /// <summary>
    /// Singleton persistente entre las 8 escenas del tutorial.
    /// Guarda estado: datos recolectados, terminales activados, checkpoint activo.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        // Estado del jugador
        public int DataCollected { get; private set; }
        public int TerminalsActivated { get; private set; }
        public const int DataGoal = 80;
        public const int TotalTerminals = 3;

        // Escenas del tutorial (en orden)
        private static readonly string[] TutorialScenes =
        {
            "T01_Materializacion",
            "T02_Disparo",
            "T03_RecoleccionDatos",
            "T04_DanoYEsquive",
            "T05_Terminal",
            "T06_PlataformasEspeciales",
            "T07_EnemigoCombinados",
            "T08_PuertoSalida"
        };

        public int CurrentBlock { get; private set; }

        // Eventos
        public System.Action<int> OnDataCollected;
        public System.Action<int> OnTerminalActivated;
        public System.Action<int> OnBlockChanged;
        public System.Action OnTutorialComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Detectar en qué bloque estamos según la escena activa
            string currentScene = SceneManager.GetActiveScene().name;
            for (int i = 0; i < TutorialScenes.Length; i++)
            {
                if (TutorialScenes[i] == currentScene)
                {
                    CurrentBlock = i;
                    break;
                }
            }
        }

        public void AddData(int amount = 1)
        {
            DataCollected += amount;
            OnDataCollected?.Invoke(DataCollected);
        }

        public void ActivateTerminal()
        {
            TerminalsActivated++;
            OnTerminalActivated?.Invoke(TerminalsActivated);
        }

        public void LoadNextBlock()
        {
            CurrentBlock++;
            if (CurrentBlock >= TutorialScenes.Length)
            {
                OnTutorialComplete?.Invoke();
                return;
            }
            OnBlockChanged?.Invoke(CurrentBlock);
            StartCoroutine(LoadSceneWithFade(TutorialScenes[CurrentBlock]));
        }

        public void LoadBlock(int index)
        {
            if (index < 0 || index >= TutorialScenes.Length) return;
            CurrentBlock = index;
            StartCoroutine(LoadSceneWithFade(TutorialScenes[index]));
        }

        private IEnumerator LoadSceneWithFade(string sceneName)
        {
            // Fade out opcional — por ahora carga directa
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene(sceneName);
        }

        // Reinicia todos los datos (para nueva partida)
        public void ResetTutorial()
        {
            DataCollected = 0;
            TerminalsActivated = 0;
            CurrentBlock = 0;
        }
    }
}
