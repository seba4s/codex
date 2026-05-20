using System.Collections;
using UnityEngine;
using CODEX.Enemies;

namespace CODEX.Tutorial.Blocks
{
    /// <summary>
    /// Bloque 2 – Sistema de Disparo (Arma de Purga).
    /// Presenta el primer Archivo Infectado estático y la puerta bloqueada.
    /// </summary>
    public class Block02_Disparo : MonoBehaviour
    {
        [SerializeField] private LumaGuide luma;
        [SerializeField] private InfectedFile blockerEnemy;
        [SerializeField] private GameObject blockedDoor;
        [SerializeField] private GameObject openDoor;
        [SerializeField] private float inactivityBeforePrompt = 8f;

        private bool enemyDefeated;

        private void Start()
        {
            if (openDoor != null) openDoor.SetActive(false);

            if (blockerEnemy != null)
                blockerEnemy.OnEnemyDeath.AddListener(_ => OnEnemyDefeated());

            StartCoroutine(RunBlock());
        }

        private IEnumerator RunBlock()
        {
            yield return new WaitForSeconds(1.5f);

            luma?.Say("Ese es un Archivo Infectado. Básicamente, basura del virus. Tu arma de purga puede limpiarlo. Usa [CLIC IZQUIERDO] o [Z] para disparar.");
            yield return new WaitForSeconds(2f);
            luma?.SayEraserOmega("Oh, mira. Enviaron a un programita a limpiar mis creaciones. Esto va a ser divertido.");

            // Si el jugador no dispara en 8 segundos, insistir
            yield return new WaitForSeconds(inactivityBeforePrompt);
            if (!enemyDefeated)
                luma?.Say("El botón es [CLIC IZQUIERDO] o [Z]. Dispara al archivo rojo bloqueando la puerta.");
        }

        private void OnEnemyDefeated()
        {
            enemyDefeated = true;

            if (blockedDoor != null) blockedDoor.SetActive(false);
            if (openDoor != null)   openDoor.SetActive(true);

            luma?.Say("¡Correcto! Eso es lo que llamamos 'purga'. Cada archivo limpiado es un paso menos para ERASER-Omega.");
        }
    }
}
