using System.Collections;
using UnityEngine;

namespace CODEX.Tutorial.Blocks
{
    /// <summary>
    /// Bloque 8 – Puerto de Salida y Cierre Narrativo.
    /// Delega el momento emocional a FirstFileKeyEvent.
    /// Al entrar al puerto USB → fade to black → NIVEL 1 COMPLETADO.
    /// </summary>
    public class Block08_PuertoSalida : MonoBehaviour
    {
        [SerializeField] private LumaGuide luma;

        private void Start()
        {
            // T08 FIX: T05 ya activó 1 terminal antes de llegar aquí.
            // Reiniciar la cuenta para que los 3 terminales de T08 sean los que disparan el cutscene.
            TutorialManager.Instance?.ResetTerminals();
            StartCoroutine(IntroDialogue());
        }

        private IEnumerator IntroDialogue()
        {
            yield return new WaitForSeconds(1f);
            // FIX B4: texto anterior era copia de Block07 ("Tercer terminal...").
            // Block08 es el puerto de salida — el jugador ya está aquí.
            luma?.Say("Puerto de salida detectado. Activa los terminales y prepárate para transferirte.");
        }
    }
}
