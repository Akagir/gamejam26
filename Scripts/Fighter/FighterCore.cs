using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FighterCore : MonoBehaviour
{
    [Header("Opponent")]
    public FighterCore opponent;

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
    public Transform attackPoint;
    public LayerMask hurtboxLayer;

    [Header("Attack Behavior")]
    public bool lockMovementDuringAttack = true;

    [Header("Light (Punch)")]
    public int lightDamage = 10;
    public int lightChipOnBlock = 0;
    public float lightHitstun = 0.25f;
    public float lightBlockstun = 0.15f;
    public float lightHitPush = 6f;
    public float lightBlockPush = 3f;
    public float lightRange = 0.60f;
    public float lightActiveTime = 0.08f;
    public float lightDuration = 0.18f;
    public float lightCooldown = 0.20f;

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

    [Header("KO Launch")]
    public float koLaunchX = 12f;   // horizontal impulse
    public float koLaunchY = 8f;    // upward impulse
    public float koMinYVel = 0f;    // optional: clear downward velocity

    [Header("State (read only)")]
    public bool isCrouching;
    public bool isBlocking;

    // internal
    Rigidbody2D rb;
    int facingDirection = 1;

    float relMoveInput;
    bool blockHeld;
    bool jumpPressed;
    bool freezeControl;

    public System.Action<FighterCore> OnKO; // fired when THIS fighter gets KO'd

    enum AttackType { None, Light, Heavy, Kick }
    AttackType queuedAttack = AttackType.None;

    bool isAttacking;
    float attackTimer;
    float activeTimer;
    float cooldownTimer;
    bool hitAlready;

    //Facing
    public int FacingDirection => facingDirection; // +1 right, -1 left

    // current attack cached
    int curDamage, curChip;
    float curHitstun, curBlockstun, curHitPush, curBlockPush, curRange;

    float stunTimer;

    // For AI/controllers to read
    public bool IsAttacking => isAttacking;
    public float MaxAttackRange => Mathf.Max(lightRange, heavyRange, kickRange);

    void Awake()
    {
        lightChipOnBlock = 0;
        heavyChipOnBlock = 0;
        kickChipOnBlock  = 0;

        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        // Always start alive
        currentHealth = maxHealth;


        isKO = false;
    }

    void LateUpdate()
    {
        if (freezeControl)
        {
            ConsumeOneFrameInputs();
            queuedAttack = AttackType.None;
            return;
        }
        
        if (stunTimer > 0f) stunTimer -= Time.deltaTime;
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        HandleFacing();

        isGrounded = Mathf.Abs(rb.linearVelocity.y) < groundedThreshold && rb.linearVelocity.y <= 0f;

        // Update attack
        if (isAttacking)
        {
            // IMPORTANT: try hit FIRST, then reduce timers
            if (!hitAlready && activeTimer > 0f)
            TryHitOpponent();

            attackTimer -= Time.deltaTime;
            activeTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
                isAttacking = false;
        }

        if (isKO)
        {
            ConsumeOneFrameInputs();
            queuedAttack = AttackType.None;
            return;
        }

        if (IsStunned())
        {
            relMoveInput = 0f;
            ConsumeOneFrameInputs();
            queuedAttack = AttackType.None;
            return;
        }

        isBlocking = blockHeld && isGrounded && !isAttacking;

        if (jumpPressed && isGrounded && !isCrouching && !isAttacking)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (queuedAttack != AttackType.None && cooldownTimer <= 0f && !isAttacking && !isBlocking)
        {
            StartAttack(queuedAttack);
        }

        ConsumeOneFrameInputs();
        queuedAttack = AttackType.None;
    }

    void FixedUpdate()
    {
        if (freezeControl)
        {
            // During KO we want to KEEP the launch motion (especially X), just block inputs.
            if (!isKO)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            return;
        }
        
        if (isKO) return;

        // don't override pushback during stun
        if (IsStunned()) return;

         if (isBlocking)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }

        if (isAttacking && lockMovementDuringAttack)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float speed = moveSpeed;

        float worldX = relMoveInput * facingDirection;
        rb.linearVelocity = new Vector2(worldX * speed, rb.linearVelocity.y);
    }

    void HandleFacing()
    {
        if (opponent == null) return;

        facingDirection = (opponent.transform.position.x >= transform.position.x) ? 1 : -1;

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * facingDirection;
        transform.localScale = s;
    }

    void StartAttack(AttackType type)
    {
        isAttacking = true;
        hitAlready = false;

        Debug.Log("{name} START ATTACK: {type}");
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
    }

    void LoadAttack(int dmg, int chip, float hitstun, float blockstun, float hitPush, float blockPush,
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

    void TryHitOpponent()
    {
        if (attackPoint == null || opponent == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, curRange, hurtboxLayer);
        if (hits == null || hits.Length == 0) return;

        FighterCore target = null;
        for (int i = 0; i < hits.Length; i++)
        {
            FighterCore fc = hits[i].GetComponentInParent<FighterCore>();
            if (fc != null && fc == opponent)
            {
                target = fc;
                break;
            }
        }

        if (target == null) return;

        hitAlready = true;
        if (target.isKO) return;

        Debug.Log($"{name} HIT {target.name} | targetBlocking={target.isBlocking} | dmg={curDamage} chip={curChip}");

        if (target.isBlocking)
        {
            target.TakeDamage(curChip, this);
            target.ApplyStun(curBlockstun);
            target.ApplyPushback(curBlockPush, transform.position.x);
            Debug.Log($"BLOCKED: chip={curChip} dmg={curDamage} blocking={target.isBlocking} grounded={target.isGrounded}");
        }
        else
        {
            target.TakeDamage(curDamage, this);
            target.ApplyStun(curHitstun);
            target.ApplyPushback(curHitPush, transform.position.x);
        }
    }

    public void TakeDamage(int amount) => TakeDamage(amount, null);

    public void TakeDamage(int amount, FighterCore attacker)
    {
        if (isKO) return;
        if (amount <= 0) return;

        int before = currentHealth;
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            EnterKO(attacker);
        }

        Debug.Log($"{name} TakeDamage({amount}) hp {before} -> {currentHealth}");
    }

    void EnterKO(FighterCore attacker)
    {
        isKO = true;
        isAttacking = false;
        stunTimer = 0f;

        // stop existing motion (then launch)
        rb.linearVelocity = Vector2.zero;

        // Cinematic launch away from attacker
        float dir = 1f;
        if (attacker != null)
            dir = (transform.position.x >= attacker.transform.position.x) ? 1f : -1f;

        // Optional: prevent "downward" feel right before launch
        float y = Mathf.Max(rb.linearVelocity.y, koMinYVel);

        rb.linearVelocity = new Vector2(0f, y);
        rb.AddForce(new Vector2(dir * koLaunchX, koLaunchY), ForceMode2D.Impulse);

        Debug.Log(name + " KO!");
        OnKO?.Invoke(this);
    }

    public void ResetFighter()
    {
        isKO = false;
        currentHealth = maxHealth;
        isAttacking = false;
        stunTimer = 0f;
        cooldownTimer = 0f;

        // IMPORTANT: clear inputs/state so FixedUpdate doesn't instantly move again
        relMoveInput = 0f;
        blockHeld = false;

        jumpPressed = false;
        queuedAttack = AttackType.None;
        isBlocking = false;
        isCrouching = false;

        rb.linearVelocity = Vector2.zero;
    }

    public void FreezeControl(bool on)
    {
        freezeControl = on;

        relMoveInput = 0f;
        blockHeld = false;
        jumpPressed = false;
        queuedAttack = AttackType.None;
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

    bool IsStunned() => stunTimer > 0f;

    void ConsumeOneFrameInputs() => jumpPressed = false;

    // -------- Feed API (controllers call these) --------
    public void SetMove(float relativeForwardBack) => relMoveInput = Mathf.Clamp(relativeForwardBack, -1f, 1f);
    public void SetBlock(bool held) => blockHeld = held;

    public void PressJump() => jumpPressed = true;
    public void PressLightPunch() => queuedAttack = AttackType.Light;
    public void PressHeavyAttack() => queuedAttack = AttackType.Heavy;
    public void PressKick() => queuedAttack = AttackType.Kick;

    void OnDrawGizmos()
    {
        if (attackPoint == null) return;

        float radius = isAttacking ? curRange : MaxAttackRange;

        bool hittingOpponent = false;
        if (opponent != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, radius, hurtboxLayer);
            foreach (var h in hits)
            {
                FighterCore fc = h.GetComponentInParent<FighterCore>();
                if (fc != null && fc == opponent) { hittingOpponent = true; break; }
            }
        }

        Gizmos.color = hittingOpponent ? Color.green : Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, radius);
    }
}