using TMPro;
using UnityEngine;

public class RoundTimerUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI messageText; // optional (TIME UP)

    [Header("Round Time")]
    public int roundSeconds = 99;

    [Header("References")]
    public FighterCore player;
    public FighterCore enemy;

    [Header("Time Scale")]
    public bool useUnscaledTime = false; // true = ignore slowmo/hitstop

    public System.Action OnTimeUp;

    int secondsLeft;
    bool running;
    float tickAccum;

    void OnEnable()
    {
        if (player) player.OnKO += OnSomeoneKO;
        if (enemy)  enemy.OnKO  += OnSomeoneKO;
    }

    void OnDisable()
    {
        if (player) player.OnKO -= OnSomeoneKO;
        if (enemy)  enemy.OnKO  -= OnSomeoneKO;
    }

    void Awake()
    {
        ResetTimer();
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!running) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        tickAccum += dt;

        while (tickAccum >= 1f && running)
        {
            tickAccum -= 1f;
            secondsLeft--;

            if (secondsLeft <= 0)
            {
                secondsLeft = 0;
                running = false;
                UpdateText();
                TimeUp();
                return;
            }

            UpdateText();
        }
    }

    void UpdateText()
    {
        if (timerText != null)
            timerText.text = secondsLeft.ToString();
    }

    void TimeUp()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = "TIME UP";
        }

        OnTimeUp?.Invoke();
    }

    void OnSomeoneKO(FighterCore whoGotKO)
    {
        StopTimer();
    }

    public void StartTimer()
    {
        secondsLeft = roundSeconds;
        tickAccum = 0f;
        running = true;

        if (messageText != null) messageText.gameObject.SetActive(false);
        UpdateText();
    }

    public void StopTimer()
    {
        running = false;
    }

    public void ResetTimer()
    {
        secondsLeft = roundSeconds;
        tickAccum = 0f;
        running = false;

        if (messageText != null) messageText.gameObject.SetActive(false);
        UpdateText();
    }

    public int GetSecondsLeft() => secondsLeft;
}