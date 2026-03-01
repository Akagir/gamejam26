using UnityEngine;
using UnityEngine.SceneManagement;

public class FightTrigger : MonoBehaviour
{
    public string fightSceneName = "FightScene";
    private bool isPlayerNear = false;

    private GameObject playerObject;

    private void Update()
    {
        if(isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            SceneTracker.PreviousScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(fightSceneName);

            if (playerObject != null)
            {
                SceneTracker.SavedPlayerPosition = playerObject.transform.position;
                SceneTracker.RestorePositionAfterChange = true;
            }

            SceneManager.LoadScene(fightSceneName);

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
