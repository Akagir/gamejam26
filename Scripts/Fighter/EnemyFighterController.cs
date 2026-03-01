using UnityEngine;

public class EnemyFighterController : MonoBehaviour
{
    [Header("Reference")]
    public FighterCore core;

    [Header("Personality")]
    [Range(0f, 1f)] public float aggression = 0.70f; // higher = attacks more
    [Range(0f, 1f)] public float defense = 0.55f;    // higher = blocks more

    [Header("Timing")]
    public float decisionRate = 0.08f;  // how often we rethink
    public float reactionTime = 0.12f;  // delay before reacting/deciding

    [Header("Spacing / Footsies")]
    public float preferredDistance = 1.0f; // should be around your max attack range
    public float tolerance = 0.15f;        // deadzone around preferred distance
    public float rangePadding = 0.15f;     // extra leniency so range isn't pixel-perfect
    public float retreatDistance = 0.65f;  // if too close, sometimes back up
    public float shimmyChance = 0.25f;     // chance to do tiny in/out movement near preferredDistance

    [Header("Approach Rule (IMPORTANT)")]
    public float approachExtra = 0.10f;    // if dist > maxR + this -> always walk in

    [Header("Attacking")]
    public float minTimeBetweenAttacks = 0.25f;
    [Range(0f, 1f)] public float attackChanceInRange = 0.80f;

    [Header("Blocking")]
    public float minBlockTime = 0.12f;
    public float maxBlockTime = 0.35f;

    // internal timers
    float decisionTimer;
    float reactionTimer;
    float blockTimer;
    float atkTimer;

    // outputs
    float relMove;    // -1 back, 0, +1 forward (relative)
    bool blockHeld;

    void Awake()
    {
        if (core == null) core = GetComponent<FighterCore>();
        decisionTimer = 0f;
        reactionTimer = reactionTime;
    }

    void Update()
    {
        if (core == null || core.opponent == null) return;
        if (core.isKO) return;

        // Timers
        if (decisionTimer > 0f) decisionTimer -= Time.deltaTime;
        if (reactionTimer > 0f) reactionTimer -= Time.deltaTime;
        if (blockTimer > 0f) blockTimer -= Time.deltaTime;
        if (atkTimer > 0f) atkTimer -= Time.deltaTime;

        FighterCore opp = core.opponent;

        // Get opponent collider (BoxCollider2D or any Collider2D)
        Collider2D oppCol = opp.GetComponent<Collider2D>();

        // Fallback if collider missing
        float dist;
        if (oppCol != null && core.attackPoint != null)
        {
            Vector2 closest = oppCol.ClosestPoint(core.attackPoint.position);
            dist = Mathf.Abs(core.attackPoint.position.x - closest.x);
        }
        else
        {
            dist = Mathf.Abs(opp.transform.position.x - core.transform.position.x);
        }
        // Ranges (with padding)
        float lightR = core.lightRange + rangePadding;
        float heavyR = core.heavyRange + rangePadding;
        float kickR  = core.kickRange  + rangePadding;
        float maxR   = Mathf.Max(lightR, Mathf.Max(heavyR, kickR));

        bool inRange = dist <= maxR;

        // ---------------------------------------------------------
        // 1) Hold block while block timer is active
        // ---------------------------------------------------------
        if (blockTimer > 0f)
        {
            blockHeld = true;
            relMove = 0f;
            Apply();
            return;
        }
        else
        {
            blockHeld = false;
        }

        // ---------------------------------------------------------
        // 2) Reaction delay (human feel)
        // ---------------------------------------------------------
        if (reactionTimer > 0f)
        {
            Apply();
            return;
        }

        // ---------------------------------------------------------
        // 3) HARD APPROACH RULE (this solves your bug)
        // If you're not in striking range, always walk forward.
        // ---------------------------------------------------------
        if (dist > maxR + approachExtra)
        {
            relMove = 1f;        // forward (relative)
            blockHeld = false;   // don't turtle while approaching
            Apply();
            return;
        }

        // ---------------------------------------------------------
        // 4) Decision step
        // ---------------------------------------------------------
        if (decisionTimer <= 0f)
        {
            decisionTimer = decisionRate;
            reactionTimer = reactionTime;

            // A) Reactive block if opponent is attacking nearby
            if (opp.IsAttacking && dist <= preferredDistance + 0.35f)
            {
                float chance = Mathf.Lerp(0.20f, 0.95f, defense);
                if (Random.value < chance)
                {
                    blockTimer = Random.Range(minBlockTime, maxBlockTime);
                    blockHeld = true;
                    relMove = 0f;
                    Apply();
                    return;
                }
            }

            // B) Attack if in range and allowed
            if (inRange && atkTimer <= 0f && !core.IsAttacking)
            {
                float pressChance = Mathf.Lerp(0.30f, attackChanceInRange, aggression);
                if (opp.isBlocking) pressChance -= 0.15f;
                pressChance = Mathf.Clamp01(pressChance);

                if (Random.value < pressChance)
                {
                    // stop moving & don't block while attacking
                    relMove = 0f;
                    blockHeld = false;

                    // Choose move by distance
                    if (dist <= lightR) core.PressLightPunch();
                    else if (dist <= heavyR) core.PressHeavyAttack();
                    else core.PressKick();

                    atkTimer = minTimeBetweenAttacks + Random.Range(0f, 0.15f);
                    Apply();
                    return;
                }
            }

            // C) Footsies (spacing)
            relMove = 0f;

            if (dist < retreatDistance)
            {
                // too close: back up sometimes (more if defensive)
                float backChance = Mathf.Lerp(0.25f, 0.85f, defense);
                relMove = (Random.value < backChance) ? -1f : 0f;
            }
            else
            {
                float delta = dist - preferredDistance;

                if (Mathf.Abs(delta) > tolerance)
                {
                    // move toward preferred distance
                    relMove = (delta > 0f) ? 1f : -1f;
                }
                else
                {
                    // within deadzone: sometimes shimmy
                    float sChance = Mathf.Lerp(0.10f, shimmyChance, aggression);
                    if (Random.value < sChance)
                        relMove = (Random.value < 0.5f) ? 1f : -1f;
                    else
                        relMove = 0f;
                }
            }
        }

        Apply();
    }

    void Apply()
    {
        core.SetMove(relMove);
        core.SetBlock(blockHeld);
    }
}