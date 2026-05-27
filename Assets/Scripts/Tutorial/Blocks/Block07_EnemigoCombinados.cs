using System.Collections;
using UnityEngine;

namespace CODEX.Tutorial.Blocks
{
    /// <summary>
    /// Bloque 7 – Enemigos Combinados.
    /// Corredor con 3 tipos de archivos infectados. LUMA no da instrucciones —
    /// el jugador usa lo aprendido. ERASER-Omega habla al entrar.
    /// </summary>
    public class Block07_EnemigoCombinados : MonoBehaviour
    {
        [SerializeField] private LumaGuide luma;

        private void Start()
        {
            StartCoroutine(IntroDialogue());
        }

        private IEnumerator IntroDialogue()
        {
            yield return new WaitForSeconds(0.5f);
            luma?.SayEraserOmega("Qué curioso. Un programa que aprende mientras corre. Interesante. Aprovecha ahora que todavía puedes avanzar.");
            yield return new WaitForSeconds(5f);
            luma?.Say("Ignóralo. Concéntrate. Hay datos en ese corredor y dos archivos infectados de tipo combinado. Ya sabes qué hacer.");
        }

        // C3 FIX (D3): IMPORTANTE — este método DEBE estar conectado vía UnityEvent en el Inspector.
        // Trigger: TutorialEnemyCounter del segundo grupo de enemigos del corredor.
        // Si no está conectado, el anuncio del tercer terminal nunca ocurre (falla silenciosa).
        // Wiring: TutorialEnemyCounter.OnAllDefeated → Block07_EnemigoCombinados.OnSecondGroupCleared()
        public void OnSecondGroupCleared()
        {
            StartCoroutine(AnnounceLastTerminal());
        }

        private IEnumerator AnnounceLastTerminal()
        {
            yield return new WaitForSeconds(2f);
            luma?.Say("Tercer terminal adelante. Después de ese, el puerto de salida. Ya casi terminas el sector.");
        }
    }
}
