using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverZone1 : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("GAME OVER");

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
