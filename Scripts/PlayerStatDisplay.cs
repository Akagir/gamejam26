using TMPro;
using UnityEngine;

public class PlayerStatDisplay : MonoBehaviour
{
    public TMP_Text affectionText;
    public TMP_Text powerupText;

    void Update()
    {
        if(affectionText != null)
            affectionText.text = "Affection: " + SceneTracker.AffectionPoints;

        if (powerupText != null)
            powerupText.text = "Power-up: " + SceneTracker.PowerupLevel;
    }
}
