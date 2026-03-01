using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FightCamera2D : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;
    public Transform enemy;

    [Header("Stage Bounds (optional but recommended)")]
    public bool clampToBounds = false;
    public Vector2 minBounds = new Vector2(-10f, -3f);
    public Vector2 maxBounds = new Vector2( 10f,  6f);

    [Header("Follow")]
    public float followSmoothTime = 0.12f;
    public Vector2 screenOffset = new Vector2(0f, 1.0f); // lift framing a bit

    [Header("Zoom (Orthographic Size)")]
    public float minOrthoSize = 4.5f;   // zoomed in
    public float maxOrthoSize = 8.5f;   // zoomed out
    public float zoomSmoothTime = 0.15f;

    [Header("Framing")]
    public float horizontalPadding = 2.0f; // world units added to distance
    public float verticalPadding   = 2.0f;
    public float maxTargetDistance = 14f;  // distance where zoom hits maxOrthoSize

    Camera cam;
    Vector3 followVelocity;
    float zoomVelocity;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    void LateUpdate()
    {
        if (!player || !enemy) return;

        // 1) Midpoint follow
        Vector3 mid = (player.position + enemy.position) * 0.5f;
        Vector3 desiredPos = new Vector3(mid.x + screenOffset.x, mid.y + screenOffset.y, transform.position.z);

        // 2) Compute required zoom based on separation
        float dx = Mathf.Abs(player.position.x - enemy.position.x) + horizontalPadding;
        float dy = Mathf.Abs(player.position.y - enemy.position.y) + verticalPadding;

        // Convert "how much must we see" into orthographic size.
        // Ortho size = half of vertical view in world units.
        float sizeByHeight = dy * 0.5f;

        // Width constraint depends on aspect: halfWidth = orthoSize * aspect
        // So orthoSize needed for width = halfWidth / aspect
        float sizeByWidth = (dx * 0.5f) / cam.aspect;

        float targetSize = Mathf.Max(sizeByHeight, sizeByWidth);

        // Optional: also map distance to size (nice feel)
        float dist = Mathf.Abs(player.position.x - enemy.position.x);
        float t = Mathf.InverseLerp(0f, maxTargetDistance, dist);
        float distMappedSize = Mathf.Lerp(minOrthoSize, maxOrthoSize, t);

        // Pick the larger requirement, then clamp
        targetSize = Mathf.Max(targetSize, distMappedSize);
        targetSize = Mathf.Clamp(targetSize, minOrthoSize, maxOrthoSize);

        // 3) Smooth zoom
        float newSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref zoomVelocity, zoomSmoothTime);
        cam.orthographicSize = newSize;

        // 4) Smooth follow (after zoom so bounds are accurate)
        Vector3 newPos = Vector3.SmoothDamp(transform.position, desiredPos, ref followVelocity, followSmoothTime);

        // 5) Clamp to stage bounds (keeps camera from showing outside level)
        if (clampToBounds)
        {
            float camHalfH = cam.orthographicSize;
            float camHalfW = camHalfH * cam.aspect;

            newPos.x = Mathf.Clamp(newPos.x, minBounds.x + camHalfW, maxBounds.x - camHalfW);
            newPos.y = Mathf.Clamp(newPos.y, minBounds.y + camHalfH, maxBounds.y - camHalfH);
        }

        transform.position = newPos;
    }
}
