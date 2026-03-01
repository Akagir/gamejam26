using UnityEngine;

public static class SceneTracker
{
    public static string PreviousScene = "MainGameScene";

    // Variables for player position tracking
    public static Vector3 SavedPlayerPosition;
    public static bool RestorePositionAfterChange = false;

    // Variables for player attribute tracking
    public static int AffectionPoints = 0;
    public static int PowerupLevel = 1;
}
