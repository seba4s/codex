using System.Collections;
using UnityEngine;

namespace CODEX.Tutorial.Blocks
{
    /// <summary>
    /// Bloque 6 – Plataformas Especiales y Sectores Dañados.
    /// Introduce CollapsingPlatform. LUMA advierte antes de que el jugador caiga.
    /// </summary>
    public class Block06_Plataformas : MonoBehaviour
    {
        [SerializeField] private LumaGuide luma;
        [SerializeField] private Transform platformSequenceStart; // punto donde empieza el abismo
        [SerializeField] private GameObject optionalDataPlatform;  // plataforma lateral con 5 datos

        private void Start()
        {
            StartCoroutine(IntroDialogue());
        }

        private IEnumerator IntroDialogue()
        {
            yield return new WaitForSeconds(1f);
            luma?.Say("Esas plataformas son sectores corruptos del disco. Están fragmentadas — no aguantan peso por mucho tiempo. Muévete rápido.");
            yield return new WaitForSeconds(4f);
            luma?.Say("Si caes, no te preocupes. El sistema tiene puntos de restauración. Pero... trata de no caer.");
        }

        // C3 FIX (D4): IMPORTANTE — este método DEBE estar conectado vía UnityEvent en el Inspector.
        // Trigger: CheckpointTrigger colocado al final de la secuencia de 4 plataformas colapsables.
        // Si no está conectado, el diálogo de cierre nunca se muestra (falla silenciosa).
        // Wiring: CheckpointTrigger.OnPlayerPassed → Block06_Plataformas.OnSequenceCleared()
        public void OnSequenceCleared()
        {
            luma?.Say("Sector cruzado. Esas plataformas existen en todo el Ciberespacio. Recuérdalo.");
        }
    }
}
