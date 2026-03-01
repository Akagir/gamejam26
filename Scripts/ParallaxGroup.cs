using UnityEngine;

public class ParallaxGroup : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Transform transform;
        [Range(0f, 1f)] public float factor = 0.2f; // smaller = slower (farther)
        [HideInInspector] public Vector3 startPos;
    }

    public Transform cam;
    public Layer[] layers;

    void Start()
    {
        if (cam == null) cam = Camera.main.transform;

        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].transform != null)
                layers[i].startPos = layers[i].transform.position;
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        Vector3 camPos = cam.position;

        for (int i = 0; i < layers.Length; i++)
        {
            var layer = layers[i];
            if (layer.transform == null) continue;

            layer.transform.position = new Vector3(
                layer.startPos.x + camPos.x * layer.factor,
                layer.startPos.y + camPos.y * layer.factor,
                layer.startPos.z
            );
        }
    }
}