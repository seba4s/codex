using System.Collections;
using UnityEngine;

namespace CODEX.Tutorial
{
    public class ShootingTarget : MonoBehaviour, CODEX.Systems.Projectile.IDamageable
    {
        [Header("Vida")]
        [SerializeField] private int maxHealth = 3;

        [Header("Feedback Visual")]
        [SerializeField] private Color hitFlashColor = Color.white;
        [SerializeField] private float flashDuration = 0.08f;

        private int currentHealth;
        private SpriteRenderer sr;
        private Color originalColor;

        public System.Action OnDestroyed;
        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) originalColor = sr.color;
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage, Vector2 sourcePosition)
        {
            if (currentHealth <= 0) return;
            currentHealth -= damage;

            if (currentHealth <= 0)
                StartCoroutine(DestroyEffect());
            else
                StartCoroutine(HitFlash());
        }

        private IEnumerator HitFlash()
        {
            if (sr == null) yield break;
            sr.color = hitFlashColor;
            yield return new WaitForSeconds(flashDuration);
            sr.color = originalColor;
        }

        private IEnumerator DestroyEffect()
        {
            if (sr != null) sr.color = hitFlashColor;

            float t = 0f;
            Vector3 startScale = transform.localScale;
            while (t < 0.18f)
            {
                t += Time.deltaTime;
                float ratio = 1f - (t / 0.18f);
                transform.localScale = startScale * ratio;
                yield return null;
            }

            OnDestroyed?.Invoke();
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
    }
}
