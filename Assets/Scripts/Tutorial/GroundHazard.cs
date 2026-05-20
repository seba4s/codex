using System.Collections;
using UnityEngine;

namespace CODEX.Tutorial
{
    public class GroundHazard : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private float activeTime   = 1f;
        [SerializeField] private float inactiveTime = 1.5f;
        [SerializeField] private int   damageAmount = 1;

        [Header("Visual")]
        [SerializeField] private GameObject visualEffect;

        private bool  isActive;
        private float damageCD;
        private int   playerMask;

        private void Start()
        {
            // Detectar todas las capas que puedan ser el jugador
            int layerPlayer  = LayerMask.NameToLayer("Player");
            int layerDefault = LayerMask.NameToLayer("Default");

            // Usar Player layer si existe, si no, usar Default como fallback
            playerMask = layerPlayer >= 0
                ? (1 << layerPlayer)
                : (1 << layerDefault);

            StartCoroutine(CycleHazard());
        }

        private IEnumerator CycleHazard()
        {
            while (true)
            {
                SetActive(true);
                yield return new WaitForSeconds(activeTime);
                SetActive(false);
                yield return new WaitForSeconds(inactiveTime);
            }
        }

        private void SetActive(bool active)
        {
            isActive = active;
            if (visualEffect != null) visualEffect.SetActive(active);
        }

        private void FixedUpdate()
        {
            if (!isActive) return;
            if (damageCD > 0f) { damageCD -= Time.fixedDeltaTime; return; }

            Vector2 size = new Vector2(transform.localScale.x * 0.9f,
                                       transform.localScale.y * 0.9f);

            Collider2D hit = Physics2D.OverlapBox(transform.position, size, 0f, playerMask);
            if (hit == null) return;

            var health = hit.GetComponentInParent<CODEX.Systems.HealthSystem>()
                      ?? hit.transform.root.GetComponent<CODEX.Systems.HealthSystem>();
            if (health == null) return;

            health.TakeDamage(damageAmount);
            damageCD = 1f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isActive ? Color.red : new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawCube(transform.position, transform.localScale);
        }
    }
}
