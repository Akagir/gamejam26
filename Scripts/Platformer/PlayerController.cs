using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;

    [Header("Movement Smoothing")]
    public float accelTime = 0.08f;  //time to reach full speed
    public float decelTime = 0.12f;  //time to come to a stop

    [Header("Air Control")]
    public float airAccelTime = 0.15f;
    public float airDecelTime = 0.20f;

    [Header("Jump")]
    public float jumpForce = 10f;
    public int maxJumps = 2;

    [Header("Jump Buffering")]
    public float jumpBufferTime = 0.12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.8f,0.5f); //width, height
    public LayerMask groundLayer;

    [Header("Dash")]
    public float dashForce = 15f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;
    public KeyCode dashKey = KeyCode.LeftShift;

    [Header("Attack")]
    public KeyCode attackKey = KeyCode.J;
    public float attackDuration = 0.2f;
    public float attackCooldown = 0.4f;

    public Transform attackPoint;         // where the attack comes from
    public float attackRange = 0.5f;      // radius of the attack
    public LayerMask enemyLayer;          // which layer is "enemy"
    public int attackDamage = 1;

    private Rigidbody2D rb;
    private float velocityXSmoothing;

    // Ground / jump
    private bool isGrounded;
    private bool wasGrounded;
    private int jumpCount;
    private float jumpBufferCounter;

    // Facing
    private int facingDirection = 1; // 1 = right, -1 = left

    // Dash
    private bool isDashing;
    private float dashCooldownTimer;
    private float originalGravity;

    // Ladder / Climb
    private bool isClimbing;
    public float climbSpeed = 4f;

    // Attack
    private bool isAttacking;
    private float attackTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        //Debug.Log("Grounded = " + isGrounded);
        CheckGround();
        // ----- Climb -----
        if (isClimbing)
        {
            float vertical = 0f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical = 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical = -1f;

            rb.linearVelocity = new Vector2(0f, vertical * climbSpeed);
            jumpBufferCounter = 0f;
            return;
        }

        // ----- Jump buffering -----
        if (Input.GetKeyDown(KeyCode.W))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter < 0f)
            jumpBufferCounter = 0f;

        // timers etc...
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (!isDashing)
        {
            HandleMovement();
            HandleJump();
        }

        HandleDash();
        HandleAttack();
    }

    void HandleMovement()
    {
        float moveInput = 0f;

        if (Input.GetKey(KeyCode.A))
            moveInput = -1f;
        else if (Input.GetKey(KeyCode.D))
            moveInput = 1f;

        // Set facing direction
        if (moveInput != 0)
            facingDirection = (int)Mathf.Sign(moveInput);

        // Target horizontal speed
        float targetSpeed = moveInput * moveSpeed;

        //Are we on ground or in air
        bool onGround = isGrounded;

        // Use acceleration when moving, deceleration when stopping
        float smoothTime;

        if(Mathf.Abs(targetSpeed) > 0.01f)
        {
            smoothTime = onGround ? accelTime : airAccelTime;
        }

        else
        {
            smoothTime = onGround ? decelTime : airDecelTime;
        }

        // Smoothly move current velocity.x towards targetSpeed
        float newx = Mathf.SmoothDamp(
            rb.linearVelocity.x,
            targetSpeed,
            ref velocityXSmoothing,
            smoothTime
        );

        rb.linearVelocity = new Vector2(newx, rb.linearVelocity.y);

    }

    void HandleJump()
    {
         // No buffered jump? do nothing
        if (jumpBufferCounter <= 0f)
            return;

        // Ground jump (first jump)
        if (isGrounded && jumpCount == 0)
        {
            DoJump();
            jumpCount = 1;          // used ground jump
            jumpBufferCounter = 0f; // consume buffer
            return;
        }

        // Air jump (second jump)
        if (!isGrounded && jumpCount == 1)
        {
            DoJump();
            jumpCount = 2;          // used air jump
            jumpBufferCounter = 0f; // consume buffer
            return;
        }
    }
    void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
    void HandleDash()
    {
        if (Input.GetKeyDown(dashKey) && dashCooldownTimer <= 0f && !isDashing)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    IEnumerator DashCoroutine()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        float previousGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Dash in facing direction
        rb.linearVelocity = new Vector2(facingDirection * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = previousGravity;
        isDashing = false;
    }

    void HandleAttack()
    {
        if (Input.GetKeyDown(attackKey) && attackTimer <= 0f && !isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        // Attack logic
        Debug.Log("ATTACK!");

        // Detect enemies
        if (attackPoint != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
                attackPoint.position,
                attackRange,
                enemyLayer
            );

            foreach (Collider2D enemy in hitEnemies)
            {
                Debug.Log("Hit: " + enemy.name);

                EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
                if (eh != null)
                {
                    eh.TakeDamage(attackDamage);
                }
            }
        }

        // 👇 THIS LINE IS CRUCIAL – it makes this a coroutine/iterator
        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
        // no explicit "return" needed – reaching the end is fine
    }

    void CheckGround()
    {
        if (groundCheck == null) return;

        bool groundedNow = Physics2D.OverlapBox(
            groundCheck.position,
            groundCheckSize,
            0f,
            groundLayer
        );

        // Reset jumpCount ONLY when we JUST landed
        if (groundedNow && !wasGrounded)
        {
            jumpCount = 0;     // now we have fresh jumps
        }

        isGrounded = groundedNow;
        wasGrounded = groundedNow;

         if (isGrounded && rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }

        if(attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
            
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ladder")) return;

        isClimbing = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Ladder")) return;

        isClimbing = false;
        rb.gravityScale = originalGravity;
    }

}