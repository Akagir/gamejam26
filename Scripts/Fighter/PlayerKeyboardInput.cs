using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerKeyboardInput : MonoBehaviour
{
    public FighterController fighter;

    [Header("Movement")]
    public KeyCode backKey = KeyCode.A;
    public KeyCode forwardKey = KeyCode.D;
    public KeyCode crouchKey = KeyCode.S;
    public KeyCode jumpKey = KeyCode.W;

    [Header("Combat")]
    public KeyCode blockKey = KeyCode.K;
    public KeyCode lightKey = KeyCode.J;
    public KeyCode heavyKey = KeyCode.I;
    public KeyCode kickKey  = KeyCode.U;

    void Update()
    {
        if (fighter == null) return;
        if (fighter.isKO) return;

        bool backHeld = Input.GetKey(backKey);
        bool forwardHeld = Input.GetKey(forwardKey);

        float rel = 0f;
        if (backHeld && !forwardHeld) rel = -1f;
        else if (forwardHeld && !backHeld) rel = 1f;

        fighter.SetMove(rel);
        fighter.SetCrouch(Input.GetKey(crouchKey));
        fighter.SetBlock(Input.GetKey(blockKey));

        if (Input.GetKeyDown(jumpKey)) fighter.PressJump();

        if (Input.GetKeyDown(lightKey)) fighter.PressLightPunch();
        if (Input.GetKeyDown(heavyKey)) fighter.PressHeavyAttack();
        if (Input.GetKeyDown(kickKey))  fighter.PressKick();
    }
}