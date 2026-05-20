using UnityEngine;
using UnityEngine.InputSystem;

namespace CODEX.Player
{
    public class ShootingSystem : MonoBehaviour
    {
        [Header("Weapon Configuration")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.15f;
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private int projectileDamage = 1;
        [SerializeField] private float projectileLifetime = 3f;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private bool canShoot = true;

        [Header("Audio")]
        [SerializeField] private AudioClip fireSound;

        private float lastFireTime;
        private AudioSource audioSource;
        private Camera mainCamera;
        private Animator animator;
        private static readonly int AnimShoot = Animator.StringToHash("Shoot");

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            mainCamera  = Camera.main;
            animator    = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        public void OnFire(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
                TryShoot();
        }

        private void Update()
        {
            if (Mouse.current.leftButton.isPressed || Keyboard.current.zKey.isPressed)
                TryShoot();
        }

        private void TryShoot()
        {
            if (!canShoot) return;
            if (projectilePrefab == null || firePoint == null) return;
            if (Time.time - lastFireTime < fireRate) return;

            lastFireTime = Time.time;
            Shoot();
        }

        private void Shoot()
        {
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 mouseWorld3 = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
            Vector2 mouseWorld = new Vector2(mouseWorld3.x, mouseWorld3.y);
            Vector2 dir = (mouseWorld - (Vector2)firePoint.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(0, 0, angle));

            CODEX.Systems.Projectile p = proj.GetComponent<CODEX.Systems.Projectile>();
            if (p != null)
            {
                p.Initialize(dir, projectileSpeed, projectileDamage, targetLayers, projectileLifetime);
            }
            else
            {
                Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.linearVelocity = dir * projectileSpeed;
            }

            animator?.SetTrigger(AnimShoot);

            if (fireSound != null && audioSource != null)
                audioSource.PlayOneShot(fireSound);
        }

        public void SetCanShoot(bool value) => canShoot = value;
    }
}
