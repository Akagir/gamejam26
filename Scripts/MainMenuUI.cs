using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Names (must match exactly)")]
    public string gameSceneName = "MainGameScene";

    [Header("Invoke Delay")]
    public float loadDelay = 0.25f;

    bool loading;

    public void Play()
    {
        if (loading) return;
        loading = true;

        Invoke(nameof(LoadGame), loadDelay);
    }

    void LoadGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit pressed (works in build only).");
    }
}
