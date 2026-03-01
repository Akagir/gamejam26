using UnityEngine;
using TMPro;

public class KOTextUI : MonoBehaviour
{
    public FighterCore player;
    public FighterCore enemy;
    public TMP_Text text;

    void Start()
    {
        if (text == null) text = GetComponent<TMP_Text>();
        if (text != null) text.enabled = false;
    }

    void Update()
    {
        if (text == null) return;

        bool show = (player != null && player.isKO) || (enemy != null && enemy.isKO);
        text.enabled = show;

        if (!show) return;

        if (player != null && player.isKO) text.text = "YOU LOSE!";
        else text.text = "KO!";
    }
}