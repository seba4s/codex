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
            StartCoroutine(IntroDialogue());
        }

        private IEnumerator IntroDialogue()
        {
            yield return new WaitForSeconds(1f);
            luma?.Say("Tercer terminal. Después de este, el puerto de salida.");
        }
    }
}
