using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FighterController : MonoBehaviour
{
    [Header("Opponent")]
    public FighterController opponent;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isKO;

    [Header("Movement")]
    public float moveSpeed = 7f;
    public float crouchSpeedMultiplier = 0.5f;

    [Header("Jump")]
    public float jumpForce = 10f;

    [Header("Grounded (velocity based)")]
    public bool isGrounded;
    public float groundedThreshold = 0.05f;

    [Header("Hit Detection")]
    public Transform attackPoint;     // empty child in front of fighter
    public LayerMask hurtboxLayer;    // set to Fighter layer

    [Header("Attack Behavior")]
    public bool lockMovementDuringAttack = true;

    // --------- LIGHT ---------
    [Header("Light (Punch)")]
    public int lightDamage = 10;
    public int lightChipOnBlock = 0;
    public float lightHitstun = 0.25f;
    public float lightBlockstun = 0.15f;
    public float lightHitPush = 6f;
    public float lightBlockPush = 3f;
    public float lightRange = 0.60f;
    public float lightActiveTime = 0.25f;
    public float lightDuration = 0.25f;
    public float lightCooldown = 0.25f;

    // --------- HEAVY ---------
    [Header("Heavy (Punch)")]
    public int heavyDamage = 18;
    public int heavyChipOnBlock = 0;
    public float heavyHitstun = 0.35f;
    public float heavyBlockstun = 0.22f;
    public float heavyHitPush = 8f;
    public float heavyBlockPush = 4f;
    public float heavyRange = 0.75f;
    public float heavyActiveTime = 0.10f;
    public float heavyDuration = 0.28f;
    public float heavyCooldown = 0.35f;

    // --------- KICK ---------
    [Header("Kick")]
    public int kickDamage = 14;
    public int kickChipOnBlock = 0;
    public float kickHitstun = 0.30f;
    public float kickBlockstun = 0.18f;
    public float kickHitPush = 7f;
    public float kickBlockPush = 3.5f;
    public float kickRange = 0.85f;
    public float kickActiveTime = 0.10f;
    public float kickDuration = 0.24f;
    public float kickCooldown = 0.28f;

    [Header("State (read only)")]
    public bool isCrouching;
    public bool isBlocking;

    // --- internal ---
    private Rigidbody2D rb;
    private int facingDirection = 1; // +1 right, -1 left

    // fed inputs
    private float relMoveInput; // -1 back, 0, +1 forward
    private bool crouchHeld;
    private bool blockHeld;
    private bool jumpPressed;

    // attacks
    private enum AttackType { None, Light, Heavy, Kick }
    private AttackType queuedAttack = AttackType.None;

    private bool isAttacking;
    private float attackTimer;
    private float activeTimer;
    private float cooldownTimer;
    private bool hitAlready;

    // current attack stats (loaded when attack starts)
    private int curDamage;
    private int curChip;
    private float curHitstun;
    private float curBlockstun;
    private float curHitPush;
    private float curBlockPush;
    private float curRange;

    private float stunTimer;

    // Useful for AI
    public bool IsAttacking => isAttacking;
    public float MaxAttackRange => Mathf.Max(lightRange, heavyRange, kickRange);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        currentHealth = maxHealth;
        isKO = false;
    }

    void Update()
    {
        if (stunTimer > 0f) stunTimer -= Time.deltaTime;
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (isKO)
        {
            ConsumeOneFrameInputs();
            queuedAttack = AttackType.None;
            return;
        }

        HandleFacing();

        isGrounded = Mathf.Abs(rb.linearVelocity.y) < groundedThreshold && rb.linearVelocity.y <= 0f;

        // update current attack
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            activeTimer -= Time.deltaTime;

            if (!hitAlready && activeTimer > 0f)
                TryHitOpponent();

            if (attackTimer <= 0f)
                isAttacking = false;
        }

        // stunned: no control
        if (IsStunned())
        {
            relMoveInput = 0f;
            ConsumeOneFrameInputs();
            queuedAttack = AttackType.None;
            return;
        }

        isCrouching = crouchHeld && isGrounded;
        isBlocking = blockHeld && isGrounded && !isAttacking;

        // jump
        if (jumpPressed && isGrounded && !isCrouching && !isAttacking)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // start queued attack
        if (queuedAttack != AttackType.None && cooldownTimer <= 0f && !isAttacking && !isBlocking)
        {
            StartAttack(queuedAttack);
        }

        ConsumeOneFrameInputs();
        queuedAttack = AttackType.None;
    }

    void FixedUpdate()
    {
        if (isKO) return;

        // IMPORTANT: don’t overwrite velocity while stunned (pushback needs to move you)
        if (IsStunned()) return;

        if (isAttacking && lockMovementDuringAttack)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float speed = moveSpeed;
        if (isCrouching) speed *= crouchSpeedMultiplier;

        float worldX = relMoveInput * facingDirection;
        rb.linearVelocity = new Vector2(worldX * speed, rb.linearVelocity.y);
    }

    private void HandleFacing()
    {
        if (opponent == null) return;

        facingDirection = (opponent.transform.position.x >= transform.position.x) ? 1 : -1;

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * facingDirection;
        transform.localScale = s;
    }

    private void StartAttack(AttackType type)
    {
        isAttacking = true;
        hitAlready = false;

        switch (type)
        {
            case AttackType.Light:
                LoadAttack(lightDamage, lightChipOnBlock, lightHitstun, lightBlockstun, lightHitPush, lightBlockPush,
                           lightRange, lightActiveTime, lightDuration, lightCooldown);
                break;

            case AttackType.Heavy:
                LoadAttack(heavyDamage, heavyChipOnBlock, heavyHitstun, heavyBlockstun, heavyHitPush, heavyBlockPush,
                           heavyRange, heavyActiveTime, heavyDuration, heavyCooldown);
                break;

            case AttackType.Kick:
                LoadAttack(kickDamage, kickChipOnBlock, kickHitstun, kickBlockstun, kickHitPush, kickBlockPush,
                           kickRange, kickActiveTime, kickDuration, kickCooldown);
                break;
        }

        Debug.Log(name + " started attack: " + type);
    }

    private void LoadAttack(int dmg, int chip, float hitstun, float blockstun, float hitPush, float blockPush,
                            float range, float active, float duration, float cooldown)
    {
        curDamage = dmg;
        curChip = chip;
        curHitstun = hitstun;
        curBlockstun = blockstun;
        curHitPush = hitPush;
        curBlockPush = blockPush;
        curRange = range;

        activeTimer = active;
        attackTimer = duration;
        cooldownTimer = cooldown;
    }

    private void TryHitOpponent()
    {
        if (attackPoint == null || opponent == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, curRange, hurtboxLayer);
        if (hits == null || hits.Length == 0) return;

        FighterController target = null;

        // Pick ONLY the opponent (ignore self / other colliders)
        for (int i = 0; i < hits.Length; i++)
        {
            FighterController fc = hits[i].GetComponentInParent<FighterController>();
            if (fc != null && fc == opponent)
            {
                target = fc;
                break;
            }
        }

        if (target == null) return;

        hitAlready = true;
        if (target.isKO) return;

        if (target.isBlocking)
        {
            target.TakeDamage(curChip);
            target.ApplyStun(curBlockstun);
            target.ApplyPushback(curBlockPush, transform.position.x);
        }
        else
        {
            target.TakeDamage(curDamage);
            target.ApplyStun(curHitstun);
            target.ApplyPushback(curHitPush, transform.position.x);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isKO) return;
        if (amount <= 0) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            EnterKO();
        }
    }

    private void EnterKO()
    {
        isKO = true;
        isAttacking = false;
        stunTimer = 0f;
        rb.linearVelocity = Vector2.zero;
        Debug.Log(gameObject.name + " KO!");
    }

    public void ResetFighter(int health = -1)
    {
        isKO = false;
        currentHealth = (health > 0) ? health : maxHealth;

        isAttacking = false;
        stunTimer = 0f;
        cooldownTimer = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    public void ApplyStun(float time)
    {
        if (isKO) return;
        stunTimer = Mathf.Max(stunTimer, time);
    }

    public void ApplyPushback(float force, float attackerX)
    {
        if (isKO) return;

        float dir = (transform.position.x > attackerX) ? 1f : -1f;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.AddForce(Vector2.right * dir * force, ForceMode2D.Impulse);
    }

    private bool IsStunned() => stunTimer > 0f;

    private void ConsumeOneFrameInputs()
    {
        jumpPressed = false;
    }

    // --------- Input Feed API ---------
    public void SetMove(float relativeForwardBack) => relMoveInput = Mathf.Clamp(relativeForwardBack, -1f, 1f);
    public void SetCrouch(bool held) => crouchHeld = held;
    public void SetBlock(bool held) => blockHeld = held;

    public void PressJump() => jumpPressed = true;

    public void PressLightPunch() => queuedAttack = AttackType.Light;
    public void PressHeavyAttack() => queuedAttack = AttackType.Heavy;
    public void PressKick() => queuedAttack = AttackType.Kick;

    void OnDrawGizmos()
    {
        if (attackPoint == null) return;

        // Determine current active range
        float radius = 0f;

        if (isAttacking)
            radius = curRange;   // current attack range
        else
            radius = Mathf.Max(lightRange, heavyRange, kickRange);

        // Check if something is inside
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            radius,
            hurtboxLayer
        );

        bool hittingOpponent = false;

        if (opponent != null)
        {
            foreach (var h in hits)
            {
                FighterController fc = h.GetComponentInParent<FighterController>();
                if (fc != null && fc == opponent)
                {
                    hittingOpponent = true;
                    break;
                }
            }
        }

        // Red = normal
        // Green = actually overlapping opponent
        Gizmos.color = hittingOpponent ? Color.green : Color.red;

        Gizmos.DrawWireSphere(attackPoint.position, radius);
    }
}