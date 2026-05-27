using UnityEngine;
using UnityEngine.Events;
using CODEX.Systems;

namespace CODEX.Tutorial
{
    // B8 FIX: REFACTOR completo — eliminado Update() con polling por frame.
    // Ahora suscribe al evento DataCollectible.OnCollected (evento-driven).
    // Ventaja: cero overhead por frame mientras los colectibles están activos.
    public class TutorialCollectionBlock : MonoBehaviour
    {
        [Header("Coleccionables")]
        [SerializeField] private DataCollectible[] collectibles;

        [Header("Puerta")]
        [SerializeField] private GameObject gate;

        [Header("Evento")]
        public UnityEvent OnAllCollected;

        private int collected;
        private int totalValid; // B8 FIX: solo colectibles no-null cuentan para el total

        private void Start()
        {
            // B8 FIX: suscripción a evento — evento-driven, no polling por frame
            // REFACTOR: eliminado Update() con escaneo manual de IsCollected
            // IMPORTANTE: DataCollectible.OnCollected se invoca en Collect()
            totalValid = 0;
            foreach (var c in collectibles)
            {
                if (c != null)
                {
                    c.OnCollected += HandleCollected;
                    totalValid++;
                }
            }
        }

        private void OnDestroy()
        {
            // B8 FIX: limpiar suscripciones para evitar callbacks a objeto destruido
            foreach (var c in collectibles)
            {
                if (c != null)
                    c.OnCollected -= HandleCollected;
            }
        }

        private void HandleCollected()
        {
            collected++;
            if (collected >= totalValid)
                Complete();
        }

        private void Complete()
        {
            if (gate != null) gate.SetActive(false);
            OnAllCollected?.Invoke();
        }
    }
}
