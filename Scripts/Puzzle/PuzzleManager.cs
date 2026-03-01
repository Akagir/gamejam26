using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    public GameObject backgroundPanel;
    public GameObject victoryPanel;
    public GameObject losePanel;

    public int goal = 200;
    public int moves = 20;
    public int points = 0;

    public bool isGameEnded;

    public TMP_Text pointsText;
    public TMP_Text goalText;
    public TMP_Text movesText;

    public PotionBoard potBoard;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(int inMoves,int inGoal)
    {
        moves = inMoves;
        points = inGoal;
    }

    void Update()
    {
        pointsText.text = "Points: " + points.ToString();
        goalText.text = "Goal: " + goal.ToString();
        movesText.text = "Remaining\nMoves: " + moves.ToString();

    }

    public void ProcessTurn(int pointsToGain,bool subtractMoves)
    {
        points += pointsToGain;
        if (subtractMoves)
            moves--;

        if(points >= goal)
        {
            isGameEnded = true;

            // Adding point and level
            SceneTracker.AffectionPoints += 10;

            backgroundPanel.SetActive(true);
            victoryPanel.SetActive(true);

            potBoard.ClearAllPotions();

            // Wait 2 seconds, then execute the ReturnToMainScene
            Invoke(nameof(ReturnToMainScene), 2f);
            return;
        }

        if(moves == 0)
        {
            isGameEnded = true;
            backgroundPanel.SetActive(true);
            losePanel.SetActive(true);

            Invoke(nameof(ReturnToMainScene), 2f);
            return;
        }
    }

    public void ReturnToMainScene()
    {
        SceneManager.LoadScene(SceneTracker.PreviousScene);
    }

    public void WinGame()
    {
        victoryPanel.SetActive(true);
    }

    public void LoseGame()
    {
        losePanel.SetActive(true);
    }
}
