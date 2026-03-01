using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleTrigger : MonoBehaviour
{
    public string puzzleSceneName = "PuzzleScene";

    private bool isPlayerNear = false;

    private GameObject playerObject;

    void Update()
    {
        // Check if player is near and button pressed
        if(isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            // Saving current scene's name before leaving
            SceneTracker.PreviousScene = SceneManager.GetActiveScene().name;

            if(playerObject != null)
            {
                SceneTracker.SavedPlayerPosition = playerObject.transform.position;
                SceneTracker.RestorePositionAfterChange = true;
            }

            // Load the puzzle scene
            SceneManager.LoadScene(puzzleSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerObject = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        isPlayerNear = false;
        playerObject = null;
    }
}
