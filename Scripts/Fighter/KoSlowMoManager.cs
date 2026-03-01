using UnityEngine;
using System.Collections;

public class KoSlowMoManager : MonoBehaviour
{
    [Header("Assign Fighters")]
    public FighterCore player;
    public FighterCore enemy;

    [Header("Hitstop (freeze)")]
    public bool enableHitstop = true;
    public float hitstopDurationRealtime = 0.06f; // real seconds
    [Range(0f, 0.2f)] public float hitstopScale = 0f; // 0 = full freeze (recommended)

    [Header("Slow Motion")]
    [Range(0.05f, 1f)] public float slowScale = 0.18f;
    public float slowDurationRealtime = 0.75f; // real seconds
    public float restoreBlendRealtime = 0.15f; // ease back

    float baseFixedDelta;
    Coroutine routine;

    void Awake()
    {
        baseFixedDelta = Time.fixedDeltaTime;
    }

    void OnEnable()
    {
        if (player) player.OnKO += HandleKO;
        if (enemy)  enemy.OnKO  += HandleKO;
    }

    void OnDisable()
    {
        if (player) player.OnKO -= HandleKO;
        if (enemy)  enemy.OnKO  -= HandleKO;
    }

    void HandleKO(FighterCore whoGotKO)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(KOTimeRoutine());
    }

    IEnumerator KOTimeRoutine()
    {
        // --- HITSTOP ---
        if (enableHitstop)
        {
            float prevScale = Time.timeScale;

            Time.timeScale = hitstopScale;
            Time.fixedDeltaTime = baseFixedDelta * Time.timeScale;

            yield return new WaitForSecondsRealtime(hitstopDurationRealtime);

            // restore to normal before slowmo (clean transition)
            Time.timeScale = prevScale;
            Time.fixedDeltaTime = baseFixedDelta * Time.timeScale;
        }

        // --- SLOW MO ---
        Time.timeScale = slowScale;
        Time.fixedDeltaTime = baseFixedDelta * Time.timeScale;

        yield return new WaitForSecondsRealtime(slowDurationRealtime);

        // --- RESTORE ---
        float t = 0f;
        float start = Time.timeScale;

        while (t < restoreBlendRealtime)
        {
            t += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(start, 1f, t / restoreBlendRealtime);
            Time.fixedDeltaTime = baseFixedDelta * Time.timeScale;
            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = baseFixedDelta;
        routine = null;
    }
}
