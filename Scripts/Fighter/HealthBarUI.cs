using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Assign")]
    public FighterCore fighter;
    public Image realFill;
    public Image delayFill;

    [Header("Delay Settings")]
    public float delayBeforeDrop = 0.25f;
    public float dropSpeed = 1.5f;

    private float lastRealPct = 1f;
    private float delayTimer = 0f;

    void Start()
    {
        if (fighter == null) return;

        float pct = GetHealthPercent();
        lastRealPct = pct;

        if (realFill != null) realFill.fillAmount = pct;
        if (delayFill != null) delayFill.fillAmount = pct;
    }

    void Update()
    {
        if (fighter == null) return;

        float realPct = GetHealthPercent();

        if (realFill != null)
            realFill.fillAmount = realPct;

        if (delayFill == null)
        {
            lastRealPct = realPct;
            return;
        }

        // Damage detected
        if (realPct < lastRealPct - 0.0001f)
            delayTimer = delayBeforeDrop;

        // Drop yellow after delay
        if (delayFill.fillAmount > realPct + 0.0001f)
        {
            if (delayTimer > 0f) delayTimer -= Time.deltaTime;
            else
                delayFill.fillAmount = Mathf.MoveTowards(delayFill.fillAmount, realPct, dropSpeed * Time.deltaTime);
        }
        else
        {
            delayFill.fillAmount = realPct;
        }

        lastRealPct = realPct;
    }

    float GetHealthPercent()
    {
        float max = Mathf.Max(1f, fighter.maxHealth);
        return Mathf.Clamp01(fighter.currentHealth / max);
    }
}