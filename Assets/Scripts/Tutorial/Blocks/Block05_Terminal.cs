using System.Collections;
using UnityEngine;

namespace CODEX.Tutorial.Blocks
{
    /// <summary>
    /// Bloque 5 – Interacción con Terminales.
    /// Explica la mecánica y abre una puerta al activar el primer terminal.
    /// </summary>
    public class Block05_Terminal : MonoBehaviour
    {
        [SerializeField] private LumaGuide luma;
        [SerializeField] private GameObject blockedDoor;
        [SerializeField] private GameObject openDoor;

        private void Start()
        {
            if (openDoor != null) openDoor.SetActive(false);

            if (TutorialManager.Instance != null)
                TutorialManager.Instance.OnTerminalActivated += HandleTerminalActivated;

            StartCoroutine(IntroDialogue());
        }

        private void OnDestroy()
        {
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.OnTerminalActivated -= HandleTerminalActivated;
        }

        private IEnumerator IntroDialogue()
        {
            yield return new WaitForSeconds(1.5f);
            luma?.Say("Terminal de reparación. Hay tres en este sector. Si los activas todos, el Disco Duro empieza a estabilizarse.");
            yield return new WaitForSeconds(4f);
            luma?.Say("Primero limpia el área. Luego acércate al terminal y presiona [E] para interactuar. Simple. O al menos debería serlo.");
        }

        private void HandleTerminalActivated(int count)
        {
            if (count == 1)
            {
                if (blockedDoor != null) blockedDoor.SetActive(false);
                if (openDoor != null)   openDoor.SetActive(true);
                luma?.Say("Uno activado. Ese sector empieza a recuperarse. Hay memorias en esos datos que Sebastian probablemente olvidó que tenía.");
            }
        }
    }
}
