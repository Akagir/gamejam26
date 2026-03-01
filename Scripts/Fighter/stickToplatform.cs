using UnityEngine;

public class StickToPlatform : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player"))
            col.transform.SetParent(transform);
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player"))
            col.transform.SetParent(null);
    }
}
