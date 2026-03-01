using UnityEngine;

public class Node
{
    public bool isUsable;
    public GameObject potion;

    public Node(bool usable, GameObject newPotion)
    {
        isUsable = usable;
        this.potion = newPotion;
    }
}
