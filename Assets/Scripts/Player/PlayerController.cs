using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace CODEX.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        #region === MOVIMIENTO ===
        [Header("Configuración de Movimiento")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float acceleration = 12f;
        [SerializeField] private float deceleration = 16f;
        #endregion

        #region === SALTO ===
        [Header("Configuración de Salto")]
        [SerializeField] private float jumpForce = 14f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 2f;
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.1f;
        [SerializeField] private float maxFallSpeed = 30f;
        #endregion

        #region === DETECCIÓN DE SUELO ===
        [Header("Detección de Suelo")]
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private float groundCheckRadius = 0.25f;
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
        [SerializeField] private LayerMask groundLayer;
        #endregion

        #region === DASH ===
        [Header("Configuración de Dash")]
        [SerializeField] private float dashSpeed = 20f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 0.8f;
        [SerializeField] private bool dashInvincible = true;
        [SerializeField] private int maxAirDashes = 1;
        #endregion

        #region === CROUCH ===
        [Header("Configuración de Agacharse")]
        #endregion

        #region === DEBUG ===
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        #endregion

        #region === ANIMACIÓN ===
        [Header("Animación")]
        [SerializeField] private Animator animator;

        private static readonly int AnimSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimIsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int AnimYVelocity = Animator.StringToHash("YVelocity");
        private static readonly int AnimJump = Animator.StringToHash("Jump");
        private static readonly int AnimDash = Animator.StringToHash("Dash");
        private static readonly int AnimHurt = Animator.StringToHash("Hurt");
        private static readonly int AnimIsCrouching = Animator.StringToHash("IsCrouching");
        #endregion

        #region === REFERENCIAS INTERNAS ===
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Collider2D col;

        private float moveInput;
        private bool jumpHeld;
        private bool facingRight = true;

        private bool isGrounded;
        private bool contactGrounded;
        private float coyoteTimer;
        private float jumpBufferTimer;
#pragma warning disable CS0414
        private bool isJumping;
#pragma warning restore CS0414

        private bool isDashing;
        private bool canDash = true;
        private float dashTimer;
        private float dashCooldownTimer;
        private int airDashesRemaining;
        private Vector2 dashDirection;
        private float originalGravityScale;

        private bool inputEnabled = true;
        private bool isInvincible;
        private bool isCrouching;

        // Input Actions
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction dashAction;
        private InputAction crouchAction;
        #endregion

        #region === EVENTOS ===
        public System.Action OnLanded;
        public System.Action OnJumped;
        public System.Action OnDashStarted;
        public System.Action OnDashEnded;
        #endregion

        #region === PROPIEDADES PÚBLICAS ===
        public bool IsGrounded => isGrounded;
        public bool IsDashing => isDashing;
        public bool IsInvincible => isInvincible;
        public bool IsCrouching => isCrouching;
        public bool FacingRight => facingRight;
        public float MoveDirection => moveInput;
        #endregion

        // ═══════════════════════════════════════════
        //  UNITY LIFECYCLE
        // ═══════════════════════════════════════════

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            originalGravityScale = rb.gravityScale;   // guardar para restaurar tras el dash

            SetupInputActions();
        }

        private void SetupInputActions()
        {
            // Move — A/D, flechas, stick izquierdo
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/a")
                .With("Positive", "<Keyboard>/d");
            moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/leftArrow")
                .With("Positive", "<Keyboard>/rightArrow");
            moveAction.AddBinding("<Gamepad>/leftStick/x");

            // Jump — Space, botón sur del gamepad
            jumpAction = new InputAction("Jump", InputActionType.Button);
            jumpAction.AddBinding("<Keyboard>/space");
            jumpAction.AddBinding("<Gamepad>/buttonSouth");

            // Dash — Left Shift, botón este del gamepad
            dashAction = new InputAction("Dash", InputActionType.Button);
            dashAction.AddBinding("<Keyboard>/leftShift");
            dashAction.AddBinding("<Gamepad>/buttonEast");

            // Crouch — S, flecha abajo, DPad abajo
            crouchAction = new InputAction("Crouch", InputActionType.Button);
            crouchAction.AddBinding("<Keyboard>/s");
            crouchAction.AddBinding("<Keyboard>/downArrow");
            crouchAction.AddBinding("<Gamepad>/dpad/down");
        }

        private void OnEnable()
        {
            moveAction.Enable();
            jumpAction.Enable();
            dashAction.Enable();
            crouchAction.Enable();

            jumpAction.performed += OnJumpPerformed;
            jumpAction.canceled += OnJumpCanceled;
            dashAction.performed += OnDashPerformed;
        }

        private void OnDisable()
        {
            jumpAction.performed -= OnJumpPerformed;
            jumpAction.canceled -= OnJumpCanceled;
            dashAction.performed -= OnDashPerformed;

            moveAction.Disable();
            jumpAction.Disable();
            dashAction.Disable();
            crouchAction.Disable();
        }

        private void Update()
        {
            if (!inputEnabled) return;

            moveInput = moveAction.ReadValue<float>();
            isCrouching = crouchAction.IsPressed() && isGrounded;

            CheckGround();
            HandleTimers();
            HandleJumpBuffer();
            HandleFlip();
            UpdateAnimator();
        }

        private void FixedUpdate()
        {
            if (isDashing)
            {
                HandleDashMovement();
                return;
            }

            if (!inputEnabled) return;

            ApplyMovement();
            ApplyBetterJumpGravity();
            ClampFallSpeed();
        }

        // ═══════════════════════════════════════════
        //  INPUT CALLBACKS
        // ═══════════════════════════════════════════

        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            if (!inputEnabled) return;

            jumpBufferTimer = jumpBufferTime;
            jumpHeld = true;

            if (showDebugLogs)
                Debug.Log($"[JUMP] Presionado | isGrounded={isGrounded} | coyoteTimer={coyoteTimer:F2}");
        }

        private void OnJumpCanceled(InputAction.CallbackContext ctx)
        {
            isJumping = false;
            jumpHeld = false;
        }

        private void OnDashPerformed(InputAction.CallbackContext ctx)
        {
            if (!inputEnabled || !canDash || isDashing) return;

            if (showDebugLogs)
                Debug.Log("[DASH] Activado");

            StartDash();
        }

        // ═══════════════════════════════════════════
        //  MOVIMIENTO HORIZONTAL
        // ═══════════════════════════════════════════

        private void ApplyMovement()
        {
            if (isCrouching) return;

            float targetSpeed = moveInput * moveSpeed;
            float speedDiff = targetSpeed - rb.linearVelocity.x;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            float movement = speedDiff * accelRate;

            rb.AddForce(Vector2.right * movement, ForceMode2D.Force);
        }

        // ═══════════════════════════════════════════
        //  SALTO Y DETECCIÓN DE SUELO
        // ═══════════════════════════════════════════

        private void CheckGround()
        {
            bool wasGrounded = isGrounded;

            Vector2 checkPos;
            if (groundCheckPoint != null)
                checkPos = groundCheckPoint.position;
            else if (col != null)
                checkPos = new Vector2(col.bounds.center.x, col.bounds.min.y + 0.05f);
            else
                checkPos = (Vector2)transform.position + groundCheckOffset;

            isGrounded = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer) || contactGrounded;

            if (isGrounded)
            {
                coyoteTimer = coyoteTime;
                airDashesRemaining = maxAirDashes;

                if (!wasGrounded)
                {
                    if (showDebugLogs)
                        Debug.Log("[GROUND] Aterrizó");
                    OnLanded?.Invoke();
                }
            }
        }

        private void HandleTimers()
        {
            if (!isGrounded)
                coyoteTimer -= Time.deltaTime;

            if (jumpBufferTimer > 0f)
                jumpBufferTimer -= Time.deltaTime;

            if (dashCooldownTimer > 0f)
            {
                dashCooldownTimer -= Time.deltaTime;
                if (dashCooldownTimer <= 0f)
                    canDash = true;
            }
        }

        private void HandleJumpBuffer()
        {
            if (isDashing) return;            // bloquear salto mientras dura el dash
            if (jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                if (showDebugLogs)
                    Debug.Log("[JUMP] ¡EJECUTANDO SALTO!");
                ExecuteJump();
            }
        }

        private void ExecuteJump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            isJumping = true;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;

            if (animator != null)
                animator.SetTrigger(AnimJump);

            OnJumped?.Invoke();
        }

        private void ApplyBetterJumpGravity()
        {
            if (rb.linearVelocity.y < 0f)
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
            }
            else if (rb.linearVelocity.y > 0f && !jumpHeld)
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
            }
        }

        private void ClampFallSpeed()
        {
            if (rb.linearVelocity.y < -maxFallSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }

        // ═══════════════════════════════════════════
        //  DASH / ESQUIVE
        // ═══════════════════════════════════════════

        private void StartDash()
        {
            if (!isGrounded)
            {
                if (airDashesRemaining <= 0) return;
                airDashesRemaining--;
            }

            dashDirection = facingRight ? Vector2.right : Vector2.left;
            isDashing = true;
            canDash = false;
            dashTimer = dashDuration;
            jumpBufferTimer = 0f;             // cancelar salto en cola al iniciar dash

            if (dashInvincible)
                isInvincible = true;

            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;

            if (animator != null)
                animator.SetTrigger(AnimDash);

            OnDashStarted?.Invoke();
        }

        private void HandleDashMovement()
        {
            dashTimer -= Time.fixedDeltaTime;

            if (dashTimer <= 0f)
            {
                EndDash();
                return;
            }

            rb.linearVelocity = dashDirection * dashSpeed;
        }

        private void EndDash()
        {
            isDashing = false;
            isInvincible = false;
            rb.gravityScale = originalGravityScale;   // restaurar gravedad del Inspector
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.3f, 0f);
            dashCooldownTimer = dashCooldown;

            OnDashEnded?.Invoke();
        }

        // ═══════════════════════════════════════════
        //  VISUAL
        // ═══════════════════════════════════════════

        private void HandleFlip()
        {
            if (isDashing) return;

            if (moveInput > 0.01f && !facingRight)
                Flip();
            else if (moveInput < -0.01f && facingRight)
                Flip();
        }

        private void Flip()
        {
            facingRight = !facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }

        private void UpdateAnimator()
        {
            if (animator == null) return;

            animator.SetFloat(AnimSpeed, Mathf.Abs(moveInput));
            animator.SetBool(AnimIsGrounded, isGrounded);
            animator.SetFloat(AnimYVelocity, rb.linearVelocity.y);
            animator.SetBool(AnimIsCrouching, isCrouching);
        }

        // ═══════════════════════════════════════════
        //  API PÚBLICA
        // ═══════════════════════════════════════════

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            if (!enabled)
            {
                moveInput = 0f;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }

        public void ApplyKnockback(Vector2 direction, float force)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);

            if (animator != null)
                animator.SetTrigger(AnimHurt);
        }

        public void TeleportTo(Vector2 position)
        {
            rb.linearVelocity = Vector2.zero;
            transform.position = position;
        }

        public void SetInvincible(float duration)
        {
            StartCoroutine(InvincibilityCoroutine(duration));
        }

        private IEnumerator InvincibilityCoroutine(float duration)
        {
            isInvincible = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (spriteRenderer != null)
                    spriteRenderer.enabled = !spriteRenderer.enabled;

                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (spriteRenderer != null)
                spriteRenderer.enabled = true;

            isInvincible = false;
        }

        // ═══════════════════════════════════════════
        //  DETECCIÓN DE SUELO POR CONTACTO
        // ═══════════════════════════════════════════

        private void OnCollisionStay2D(Collision2D collision)
        {
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    contactGrounded = true;
                    return;
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            contactGrounded = false;
        }

        // ═══════════════════════════════════════════
        //  GIZMOS
        // ═══════════════════════════════════════════

        private void OnDrawGizmosSelected()
        {
            Vector2 checkPos;
            if (groundCheckPoint != null)
                checkPos = groundCheckPoint.position;
            else
            {
                Collider2D c = GetComponent<Collider2D>();
                if (c != null)
                    checkPos = new Vector2(c.bounds.center.x, c.bounds.min.y + 0.05f);
                else
                    checkPos = (Vector2)transform.position + groundCheckOffset;
            }

            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(checkPos, groundCheckRadius);
        }
    }
}