using System.Collections;
using TMPro;
using UnityEngine;

public class RoundStartUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI startText;

    [Header("Timing")]
    public float readyTime = 1.0f;
    public float goTime = 0.6f;

    [Header("Control (optional)")]
    public FighterCore player;
    public FighterCore enemy;

    public RoundTimerUI roundTimer;

    void Awake()
    {
        if (startText != null)
            startText.gameObject.SetActive(false);
    }

    public void StartRound()
    {
        StopAllCoroutines();
        StartCoroutine(StartRoutine());
    }

    void Start() => StartRound();

    IEnumerator StartRoutine()
    {
        // Freeze fighters (only if you have FreezeControl in FighterCore)
        if (player) player.FreezeControl(true);
        if (enemy)  enemy.FreezeControl(true);

        startText.gameObject.SetActive(true);

        startText.text = "READY";
        yield return new WaitForSecondsRealtime(readyTime);

        startText.text = "GO";
        yield return new WaitForSecondsRealtime(goTime);

        startText.gameObject.SetActive(false);

        if (player) player.FreezeControl(false);
        if (enemy)  enemy.FreezeControl(false);
        if (roundTimer != null) roundTimer.StartTimer();
    }
}