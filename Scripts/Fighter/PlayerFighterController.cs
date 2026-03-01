using UnityEngine;

public class PlayerFİghterController : MonoBehaviour
{
    public FighterCore core;

    [Header("Keys")]
    public KeyCode backKey = KeyCode.A;
    public KeyCode forwardKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;

    public KeyCode blockKey = KeyCode.S;
    public KeyCode lightKey = KeyCode.J;
    public KeyCode heavyKey = KeyCode.I;
    public KeyCode kickKey  = KeyCode.U;

    void Awake()
    {
        if (core == null) core = GetComponent<FighterCore>();
    }

    void Update()
    {
    if (core == null) return;
    if (core.isKO) return;

    // WORLD input (left/right)
    bool leftHeld  = Input.GetKey(backKey);      // A
    bool rightHeld = Input.GetKey(forwardKey);   // D

    float world = 0f;
    if (leftHeld && !rightHeld) world = -1f;
    else if (rightHeld && !leftHeld) world = 1f;

    // Convert to RELATIVE (forward/back)
    float rel = world * core.FacingDirection;
    core.SetMove(rel);

    // Other inputs (unchanged)
    core.SetBlock(Input.GetKey(blockKey));

    if (Input.GetKeyDown(jumpKey)) core.PressJump();
    if (Input.GetKeyDown(lightKey)) core.PressLightPunch();
    if (Input.GetKeyDown(heavyKey)) core.PressHeavyAttack();
    if (Input.GetKeyDown(kickKey))  core.PressKick();
}
}