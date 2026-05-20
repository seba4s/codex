using UnityEngine;
using UnityEngine.Events;
using CODEX.Systems;

namespace CODEX.Tutorial
{
    public class TutorialCollectionBlock : MonoBehaviour
    {
        [Header("Coleccionables")]
        [SerializeField] private DataCollectible[] collectibles;

        [Header("Puerta")]
        [SerializeField] private GameObject gate;

        [Header("Evento")]
        public UnityEvent OnAllCollected;

        private int collected;

        private void Start()
        {
            foreach (var c in collectibles)
            {
                if (c != null)
                {
                    // Polling-based check since DataCollectible has no OnCollected event
                    // We check every frame until all are collected
                }
            }
        }

        private void Update()
        {
            if (collected >= collectibles.Length) return;

            int count = 0;
            foreach (var c in collectibles)
            {
                if (c == null || c.IsCollected) count++;
            }

            if (count > collected)
            {
                collected = count;
                if (collected >= collectibles.Length)
                    Complete();
            }
        }

        private void Complete()
        {
            if (gate != null) gate.SetActive(false);
            OnAllCollected?.Invoke();
        }
    }
}
