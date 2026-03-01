using UnityEngine;

public class CameraShaker2D : MonoBehaviour
{
    [Header("Default Shake")]
    public float defaultDuration = 0.10f;
    public float defaultStrength = 0.20f;
    public float defaultFrequency = 25f;

    Vector3 baseLocalPos;
    float timer;
    float strength;
    float frequency;
    int seed;

    void Awake()
    {
        baseLocalPos = transform.localPosition;
        seed = Random.Range(0, 99999);
    }

    void LateUpdate()
    {
        // If your follow camera moves the camera in world-space,
        // we shake in local-space offset on top of it.
        if (timer > 0f)
        {
            timer -= Time.deltaTime;

            float t = timer / Mathf.Max(0.0001f, defaultDuration);
            float damp = t; // linear falloff

            float nx = (Mathf.PerlinNoise(seed, Time.time * frequency) - 0.5f) * 2f;
            float ny = (Mathf.PerlinNoise(seed + 13, Time.time * frequency) - 0.5f) * 2f;

            Vector3 offset = new Vector3(nx, ny, 0f) * (strength * damp);
            transform.localPosition = baseLocalPos + offset;
        }
        else
        {
            transform.localPosition = baseLocalPos;
        }
    }

    public void Shake(float duration, float strength, float frequency = 25f)
    {
        this.timer = Mathf.Max(this.timer, duration);
        this.strength = Mathf.Max(this.strength, strength);
        this.frequency = frequency;
        this.defaultDuration = duration; // so damp uses the new duration
    }

    // Convenience
    public void ShakeDefault() => Shake(defaultDuration, defaultStrength, defaultFrequency);

    // If something else changes camera parent/position at runtime:
    public void RecalibrateBase() => baseLocalPos = transform.localPosition;
}