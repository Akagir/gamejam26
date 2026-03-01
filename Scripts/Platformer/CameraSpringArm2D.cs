using UnityEngine;

public class CameraSpringArm2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;          // Player

    [Header("Offset")]
    public Vector2 offset = new Vector2(0f, 1f); // how much above/aside the player
    public float cameraZ = -10f;      // keep this -10 for 2D
    
    [Header("Spring Settings")]
    public float springStrength = 50f; // how strongly the camera pulls toward target
    public float damping = 10f;        // how quickly it settles (friction)

    [Header("Look Ahead")]
    public float lookAheadDistance = 2f;
    public float lookAheadSmooth = 10f;

    private Vector3 velocity;          // internal spring velocity
    private float currentLookAheadX;   // smoothed look-ahead value

    void LateUpdate()
    {
        if (target == null)
            return;

        // ---------- LOOK AHEAD BASED ON INPUT ----------
        float inputX = 0f;

        if (Input.GetKey(KeyCode.A))
            inputX = -1f;
        else if (Input.GetKey(KeyCode.D))
            inputX = 1f;

        float targetLookAheadX = 0f;

        // only look ahead while a direction key is actually held
        if (Mathf.Abs(inputX) > 0.01f)
        {
            targetLookAheadX = inputX * lookAheadDistance;
        }

        // smooth the look-ahead so it doesn't snap
        currentLookAheadX = Mathf.Lerp(
            currentLookAheadX,
            targetLookAheadX,
            lookAheadSmooth * Time.deltaTime
        );

        // ---------- SPRING FOLLOW (Y UNCHANGED) ----------
        // Where we WANT the camera to be (target + offset + lookAhead, fixed Z)
        Vector3 desiredPos = new Vector3(
            target.position.x + offset.x + currentLookAheadX,
            target.position.y + offset.y,   // <- Y stays exactly as before
            cameraZ
        );

        // Spring physics: F = kx style
        Vector3 displacement = desiredPos - transform.position;
        Vector3 springForce = displacement * springStrength;

        // Integrate velocity with spring force
        velocity += springForce * Time.deltaTime;

        // Apply damping (friction)
        velocity *= 1f / (1f + damping * Time.deltaTime);

        // Move the camera
        transform.position += velocity * Time.deltaTime;
    }
}