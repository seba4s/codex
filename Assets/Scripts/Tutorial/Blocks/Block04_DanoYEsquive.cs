using System.Collections;
using UnityEngine;

namespace CODEX.Tutorial.Blocks
{
    /// <summary>
    /// Bloque 4 – Daño, Salud y Esquive.
    /// El indicador de esquive aparece SOLO después del primer impacto.
    /// </summary>
    public class Block04_DanoYEsquive : MonoBehaviour
    {
        [SerializeField] private LumaGuide luma;
        [SerializeField] private TutorialHUD hud;

        private bool dashTipShown;

        private void Start()
        {
            if (luma == null) luma = Object.FindAnyObjectByType<LumaGuide>();
            if (hud  == null) hud  = Object.FindAnyObjectByType<TutorialHUD>();

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var health = player.GetComponent<CODEX.Systems.HealthSystem>();
                if (health != null)
                    health.OnDamaged += HandlePlayerDamaged;
            }

            StartCoroutine(IntroDialogue());
        }

        private IEnumerator IntroDialogue()
        {
            yield return new WaitForSeconds(1.5f);
            luma?.Say("LUMA", "Esos chorros de energía corruptora te dañan al contacto. Tu barra de integridad baja con cada golpe. Si llega a cero... bueno, no llegues a cero.");
            yield return new WaitForSeconds(6f);
            luma?.Say("LUMA", "Cruza hasta la salida al fondo. Aprende el ritmo — se apagan cada 1.5 segundos.");
        }

        private void HandlePlayerDamaged()
        {
            if (dashTipShown) return;
            dashTipShown = true;

            hud?.ShowDashIndicator();

            luma?.Say("LUMA", "¡Te golpearon! Usa [SHIFT] para esquivar. Te da un instante de invulnerabilidad.");
            StartCoroutine(DashConfirmDialogue());
        }

        private IEnumerator DashConfirmDialogue()
        {
            yield return new WaitForSeconds(8f);
            luma?.Say("LUMA", "El esquive te hace invulnerable por un instante. Úsalo justo antes de que el chorro se active.");
        }
    }
}
