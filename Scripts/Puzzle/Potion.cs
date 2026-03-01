using System.Collections;
using UnityEngine;

public class Potion : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    public PotionType potionType;
    public bool matched;

    private Vector2 currentPos;
    private Vector2 targetPos;

    public bool isMoving;


    public Potion(int xPos,int yPos)
    {
        xIndex = xPos;
        yIndex = yPos;
    }

    public void SetIndicies(int xPos,int yPos)
    {
        xIndex = xPos;
        yIndex = yPos;
    }

    //Move to Target
    public void MoveToTarget(Vector2 inTargetPos)
    {
        StartCoroutine(MoveCoroutine(inTargetPos));
    }

    //Move Coroutine
    private IEnumerator MoveCoroutine(Vector2 inTargetPos)
    {
        isMoving = true;
        float duration = 0.2f;
        Vector2 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = Time.deltaTime/duration;
            transform.position = Vector2.Lerp(startPosition, inTargetPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = inTargetPos;
        isMoving = false;
    }
}
public enum PotionType
{ Red, Blue, Purple, Green, White }