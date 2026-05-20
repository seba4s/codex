using System.Collections;
using UnityEngine;

namespace CODEX.Tutorial.Blocks
{
    public class Block03_RecoleccionDatos : MonoBehaviour
    {
        [SerializeField] private LumaGuide luma;
        [SerializeField] private GameObject openDoor;
        [SerializeField] private int dataRequired = 5;

        private bool doorOpened;

        private void Start()
        {
            if (openDoor != null) openDoor.SetActive(false);

            if (TutorialManager.Instance != null)
                TutorialManager.Instance.OnDataCollected += OnDataCollected;

            StartCoroutine(IntroDialogue());
        }

        private void OnDestroy()
        {
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.OnDataCollected -= OnDataCollected;
        }

        private IEnumerator IntroDialogue()
        {
            yield return new WaitForSeconds(1f);
            luma?.Say("LUMA", "¿Ves esos fragmentos de luz? Son datos del sistema. Recógelos todos para abrir la salida. Dentro hay archivos de Sebastian — fotos, mensajes. Todo eso está en riesgo.");
            yield return new WaitForSeconds(5f);
            luma?.Say("LUMA", $"Recoge los {dataRequired} fragmentos para continuar.");
        }

        private void OnDataCollected(int total)
        {
            if (doorOpened) return;

            if (total >= dataRequired)
            {
                doorOpened = true;
                if (openDoor != null) openDoor.SetActive(true);
                luma?.Say("LUMA", "¡Todos los fragmentos recolectados! La salida está abierta.");
            }
        }
    }
}
