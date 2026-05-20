using UnityEngine;
using TMPro;

namespace CODEX.Tutorial
{
    /// <summary>
    /// HUD progresivo del tutorial. Los elementos aparecen sólo cuando el jugador
    /// los necesita, no todos desde el inicio. Escucha al TutorialManager.
    /// </summary>
    public class TutorialHUD : MonoBehaviour
    {
        [Header("Integridad (salud) — siempre visible")]
        [SerializeField] private GameObject[] healthSegments;  // 5 segmentos

        [Header("Contador de Datos — aparece al recoger el 1er dato")]
        [SerializeField] private GameObject dataCounterRoot;
        [SerializeField] private TextMeshProUGUI dataCounterText;

        [Header("Terminales — aparece al activar el 1er terminal")]
        [SerializeField] private GameObject terminalCounterRoot;
        [SerializeField] private TextMeshProUGUI terminalCounterText;

        [Header("Indicador de esquive — aparece en Bloque 4")]
        [SerializeField] private GameObject dashIndicatorRoot;

        private CODEX.Systems.HealthSystem health;
        private TutorialManager tm;

        private void Start()
        {
            // Ocultar elementos hasta que sean necesarios
            if (dataCounterRoot != null)    dataCounterRoot.SetActive(false);
            if (terminalCounterRoot != null) terminalCounterRoot.SetActive(false);
            if (dashIndicatorRoot != null)   dashIndicatorRoot.SetActive(false);

            tm = TutorialManager.Instance;
            if (tm != null)
            {
                tm.OnDataCollected += HandleDataCollected;
                tm.OnTerminalActivated += HandleTerminalActivated;
            }

            // Buscar el sistema de salud del jugador
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                health = player.GetComponent<CODEX.Systems.HealthSystem>();
                if (health != null)
                {
                    health.OnHealthChanged += UpdateHealthBar;
                    UpdateHealthBar(health.CurrentHealth, health.MaxHealth);
                }
            }

            RefreshAll();
        }

        private void OnDestroy()
        {
            if (tm != null)
            {
                tm.OnDataCollected -= HandleDataCollected;
                tm.OnTerminalActivated -= HandleTerminalActivated;
            }
            if (health != null)
                health.OnHealthChanged -= UpdateHealthBar;
        }

        private void HandleDataCollected(int count)
        {
            if (dataCounterRoot != null && !dataCounterRoot.activeSelf)
                dataCounterRoot.SetActive(true);

            if (dataCounterText != null)
                dataCounterText.text = $"{count} / {TutorialManager.DataGoal}";
        }

        private void HandleTerminalActivated(int count)
        {
            if (terminalCounterRoot != null && !terminalCounterRoot.activeSelf)
                terminalCounterRoot.SetActive(true);

            if (terminalCounterText != null)
                terminalCounterText.text = $"Terminales  {count}/{TutorialManager.TotalTerminals}";
        }

        private void UpdateHealthBar(int current, int max)
        {
            for (int i = 0; i < healthSegments.Length; i++)
            {
                if (healthSegments[i] != null)
                    healthSegments[i].SetActive(i < current);
            }
        }

        public void ShowDashIndicator() { if (dashIndicatorRoot != null) dashIndicatorRoot.SetActive(true); }

        private void RefreshAll()
        {
            if (tm == null) return;
            if (tm.DataCollected > 0) HandleDataCollected(tm.DataCollected);
            if (tm.TerminalsActivated > 0) HandleTerminalActivated(tm.TerminalsActivated);
        }
    }
}
