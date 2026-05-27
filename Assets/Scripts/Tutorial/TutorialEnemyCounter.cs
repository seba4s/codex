using UnityEngine;
using UnityEngine.Events;
using CODEX.Enemies;

namespace CODEX.Tutorial
{
    public class TutorialEnemyCounter : MonoBehaviour
    {
        [Header("Enemigos")]
        [SerializeField] private InfectedFile[] enemies;

        [Header("Puerta")]
        [SerializeField] private GameObject gate;

        [Header("Evento")]
        public UnityEvent OnAllEnemiesDefeated;

        private int defeated;

        // D2 FIX (F3): propiedad pública para que TutorialSceneLoader pueda consultar el estado
        // sin polling — lectura directa de campo en el momento del trigger.
        public bool AllDefeated => enemies.Length > 0 && defeated >= enemies.Length;

        private void Start()
        {
            foreach (var e in enemies)
            {
                if (e != null)
                    e.OnEnemyDeath.AddListener(HandleEnemyDeath);
            }
        }

        private void HandleEnemyDeath(InfectedFile _)
        {
            defeated++;
            if (defeated >= enemies.Length)
            {
                if (gate != null) gate.SetActive(false);
                OnAllEnemiesDefeated?.Invoke();
            }
        }
    }
}
