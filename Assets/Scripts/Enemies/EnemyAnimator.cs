using UnityEngine;

namespace CODEX.Enemies
{
    /// <summary>
    /// Lee el estado de InfectedFile y actualiza el Animator del enemigo.
    /// Parámetros del controller: Speed (float), IsShooting (bool), Die (trigger).
    /// </summary>
    [RequireComponent(typeof(Animator), typeof(InfectedFile))]
    public class EnemyAnimator : MonoBehaviour
    {
        private Animator anim;
        private InfectedFile enemy;
        private Rigidbody2D rb;

        private static readonly int ParamSpeed      = Animator.StringToHash("Speed");
        private static readonly int ParamIsShooting = Animator.StringToHash("IsShooting");
        private static readonly int ParamDie        = Animator.StringToHash("Die");

        private bool deathFired;

        private bool hasController;

        private void Awake()
        {
            anim  = GetComponent<Animator>();
            enemy = GetComponent<InfectedFile>();
            rb    = GetComponent<Rigidbody2D>();
            hasController = anim.runtimeAnimatorController != null;
            if (!hasController)
                Debug.LogWarning("[EnemyAnimator] Sin AnimatorController en " + gameObject.name +
                                 ". Asigna EnemyController.controller al Animator.", this);
        }

        private void Start()
        {
            enemy.OnEnemyDeath.AddListener(_ => TriggerDeath());
            enemy.OnAttack.AddListener(TriggerShoot);
        }

        private void Update()
        {
            if (!hasController || enemy.IsDead) return;

            float speed = rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;
            anim.SetFloat(ParamSpeed, speed);
        }

        private void TriggerShoot()
        {
            if (!hasController || enemy.IsDead) return;
            anim.SetBool(ParamIsShooting, true);
            Invoke(nameof(StopShoot), 0.4f);
        }

        private void StopShoot()
        {
            if (hasController) anim.SetBool(ParamIsShooting, false);
        }

        private void TriggerDeath()
        {
            if (!hasController || deathFired) return;
            deathFired = true;
            anim.SetTrigger(ParamDie);
        }
    }
}
