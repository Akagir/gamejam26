using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    [Header("Refs")]
    public FighterCore player;
    public FighterCore enemy;
    public MatchManager matchManager;
    public RoundStartUI roundStartUI;
    public RoundTimerUI roundTimer;

    [Header("UI (optional)")]
    public TextMeshProUGUI resultText; // "PLAYER WINS", "ENEMY WINS", "DRAW", "MATCH WIN"
    public float resultShowTime = 1.2f;

    [Header("Rules")]
    public int roundsToWin = 2; // Best of 3

    int playerRounds;
    int enemyRounds;
    bool roundOver;
    bool matchOver;
    Coroutine flow;

    void OnEnable()
    {
        if (player) player.OnKO += HandleKO;
        if (enemy)  enemy.OnKO  += HandleKO;

        if (roundTimer) roundTimer.OnTimeUp += HandleTimeUp;
    }

    void OnDisable()
    {
        if (player) player.OnKO -= HandleKO;
        if (enemy)  enemy.OnKO  -= HandleKO;

        if (roundTimer) roundTimer.OnTimeUp -= HandleTimeUp;
    }

    void Start()
    {
        if (resultText) resultText.gameObject.SetActive(false);
        StartMatchFresh();
    }

    void Update()
    {
        // Press MatchManager reset key to restart whole match
        if (matchManager != null && Input.GetKeyDown(matchManager.resetKey))
        {
            StartMatchFresh();
        }
    }

    void StartMatchFresh()
    {
        if (flow != null) StopCoroutine(flow);

        matchOver = false;
        roundOver = false;
        playerRounds = 0;
        enemyRounds = 0;

        if (resultText) resultText.gameObject.SetActive(false);

        if (roundTimer) roundTimer.ResetTimer();
        if (matchManager) matchManager.ResetMatch();
        if (roundStartUI) roundStartUI.StartRound();
    }

    void HandleKO(FighterCore whoGotKO)
    {
        if (roundOver || matchOver) return;
        roundOver = true;

        // Winner is the other fighter
        bool playerWon = (whoGotKO == enemy);
        AwardRound(playerWon ? Winner.Player : Winner.Enemy);
    }

    void HandleTimeUp()
    {
        if (roundOver || matchOver) return;
        roundOver = true;

        int p = player ? player.currentHealth : 0;
        int e = enemy ? enemy.currentHealth : 0;

        if (p > e) AwardRound(Winner.Player);
        else if (e > p) AwardRound(Winner.Enemy);
        else AwardRound(Winner.Draw);
    }

    enum Winner { Player, Enemy, Draw }

    void AwardRound(Winner winner)
    {
        if (flow != null) StopCoroutine(flow);
        flow = StartCoroutine(AwardRoutine(winner));
    }

    IEnumerator AwardRoutine(Winner winner)
    {
        if (roundTimer) roundTimer.StopTimer();

        if (player) player.FreezeControl(true);
        if (enemy)  enemy.FreezeControl(true);

        string msg =
            winner == Winner.Player ? "PLAYER WINS" :
            winner == Winner.Enemy ? "ENEMY WINS" :
            "DRAW";

        if (winner == Winner.Player) playerRounds++;
        if (winner == Winner.Enemy) enemyRounds++;

        if (resultText)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = msg;
        }

        yield return new WaitForSecondsRealtime(resultShowTime);

        if (resultText) resultText.gameObject.SetActive(false);

        // Match win?
        if (playerRounds >= roundsToWin || enemyRounds >= roundsToWin)
        {
            matchOver = true;

            string matchMsg = (playerRounds >= roundsToWin) ? "PLAYER WINS MATCH" : "ENEMY WINS MATCH";
            if (resultText)
            {
                resultText.gameObject.SetActive(true);
                resultText.text = matchMsg;
            }

            MatchHasFinished();
            
            // Stay frozen; press R to restart (handled in Update)
            yield break;
        }
        // Next round
        if (roundTimer) roundTimer.ResetTimer();
        if (matchManager) matchManager.ResetMatch();
        if (roundStartUI) roundStartUI.StartRound();

        roundOver = false;
    }

    private void MatchHasFinished()
    {
        SceneTracker.AffectionPoints += 5;
        SceneTracker.PowerupLevel += 1;

        Invoke(nameof(ReturnToMainScene), 2f);
    }

    public void ReturnToMainScene()
    {
        SceneManager.LoadScene(SceneTracker.PreviousScene);
    }
}