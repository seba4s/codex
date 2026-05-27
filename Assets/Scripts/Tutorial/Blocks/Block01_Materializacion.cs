using System.Collections;
using UnityEngine;

namespace CODEX.Tutorial.Blocks
{
    /// <summary>
    /// Bloque 1 – Materialización y Movimiento Básico.
    /// Controla la pantalla de diagnóstico, el diálogo inicial de LUMA
    /// y activa el detector de inactividad para el primer salto.
    /// </summary>
    public class Block01_Materializacion : MonoBehaviour
    {
        [SerializeField] private DiagnosticBootScreen bootScreen;
        [SerializeField] private LumaGuide luma;
        // C4 FIX (F5): playerPrefab y spawnPoint eliminados — nunca se usaron en RunBlock().
        // El jugador de T01 existe en la escena desde el inicio; no se spawna dinámicamente.
        // Si en el futuro se necesita spawn procedural, reimplementar aquí con CheckpointManager.
        [SerializeField] private GameObject doorToT02;

        private void Start()
        {
            StartCoroutine(RunBlock());
        }

        private IEnumerator RunBlock()
        {
            // 1. Pantalla de diagnóstico
            if (bootScreen != null)
            {
                bool bootDone = false;
                bootScreen.OnBootComplete += () => bootDone = true;
                yield return new WaitUntil(() => bootDone);
            }

            // Dar 5–8 segundos para que el jugador explore
            yield return new WaitForSeconds(5f);

            // 2. LUMA saluda
            luma?.Say("Bien. Estás activo. Soy LUMA, tu interfaz de navegación. Y ya sé que no pediste guía, pero alguien tiene que decirte cómo funciona esto.");
            yield return new WaitForSeconds(5f);
            luma?.Say("Este lugar es el Disco Duro. Todo lo que ves aquí son archivos, carpetas, memoria. Y están en peligro. ¿Puedes moverte?");

            // 3. Activar detector de inactividad para el primer salto
            yield return new WaitForSeconds(3f);
            luma?.EnableIdleNudge("...¿Vas a quedarte ahí todo el día? Hay una plataforma justo enfrente.");

            // 4. Esperar a que LUMA termine de hablar y abrir la puerta al siguiente sector
            yield return new WaitUntil(() => luma == null || !luma.IsTalking);
            yield return new WaitForSeconds(0.5f);
            luma?.Say("El acceso al siguiente sector está desbloqueado. Dirígete a la derecha.");
            yield return new WaitUntil(() => luma == null || !luma.IsTalking);
            if (doorToT02 != null) doorToT02.SetActive(true);
        }
    }
}
