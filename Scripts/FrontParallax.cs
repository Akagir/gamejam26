using UnityEngine;

public class FrontParallax : MonoBehaviour
{
    [Header("Assign")]
    [SerializeField] private Transform cam;

    [Header("Parallax Strength")]
    [Tooltip("Horizontal strength (use >1 for foreground).")]
    [SerializeField] private float xMultiplier = 1.5f;

    [Tooltip("Vertical strength.")]
    [SerializeField] private float yMultiplier = 1.0f;

    [Header("Axis Control")]
    [SerializeField] private bool useX = true;
    [SerializeField] private bool useY = true;

    [Header("Direction")]
    [Tooltip("Invert movement direction.")]
    [SerializeField] private bool invert = true;

    [Header("Optional Smoothing")]
    [Tooltip("0 = no smoothing.")]
    [SerializeField] private float smooth = 0f;

    private Vector3 _lastCamPos;
    private Vector3 _velocity;

    private void Awake()
    {
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;
    }

    private void Start()
    {
        if (cam == null)
        {
            Debug.LogError("FrontParallax: No camera assigned.");
            enabled = false;
            return;
        }

        _lastCamPos = cam.position;
    }

    private void LateUpdate()
    {
        Vector3 camDelta = cam.position - _lastCamPos;

        float direction = invert ? -1f : 1f;

        Vector3 movement = new Vector3(
            useX ? camDelta.x * xMultiplier : 0f,
            useY ? camDelta.y * yMultiplier : 0f,
            0f
        ) * direction;

        Vector3 targetPos = transform.position + movement;

        if (smooth > 0f)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPos,
                ref _velocity,
                1f / smooth
            );
        }
        else
        {
            transform.position = targetPos;
        }

        _lastCamPos = cam.position;
    }
}