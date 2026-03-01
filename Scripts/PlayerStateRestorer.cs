using UnityEngine;

public class PlayerStateRestorer : MonoBehaviour
{
    
    void Start()
    {
        if(SceneTracker.RestorePositionAfterChange)
        {
            transform.position = SceneTracker.SavedPlayerPosition;

            SceneTracker.RestorePositionAfterChange = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
