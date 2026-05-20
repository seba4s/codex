using UnityEngine;
using CODEX.Player;

namespace CODEX.Tutorial
{
    [RequireComponent(typeof(Collider2D))]
    public class SpikeDamage : MonoBehaviour
    {
        [SerializeField] private int damage = 1;

        private void Start() => GetComponent<Collider2D>().isTrigger = true;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            var ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(damage, transform.position);
        }
    }
}
