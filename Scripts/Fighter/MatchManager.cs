using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public FighterCore player;
    public FighterCore enemy;

    [Header("Reset Key")]
    public KeyCode resetKey = KeyCode.R;

    [Header("Optional spawn positions")]
    public bool resetPositions = true;
    public Vector2 playerSpawn = new Vector2(-2f, 0f);
    public Vector2 enemySpawn  = new Vector2( 2f, 0f);

    void Update()
    {
        if (Input.GetKeyDown(resetKey))
        {
            ResetMatch();
        }
    }

    public void ResetMatch()
    {
        if (player != null) player.ResetFighter();
        if (enemy  != null) enemy.ResetFighter();

        if (resetPositions)
        {
            if (player != null)
                player.transform.position = new Vector3(playerSpawn.x, playerSpawn.y, player.transform.position.z);

            if (enemy != null)
                enemy.transform.position = new Vector3(enemySpawn.x, enemySpawn.y, enemy.transform.position.z);
        }

        Debug.Log("MATCH RESET");
    }
}