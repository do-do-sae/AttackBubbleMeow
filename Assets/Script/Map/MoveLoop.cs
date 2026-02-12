using UnityEngine;

public class CityParallaxController : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Transform root;              // Sky/Far/Mid/Near/Lights (타일 2장을 가진 부모)
        [Range(0f, 1f)] public float multiplier = 0.5f;
        public float repeatWidth = 25f;     // 그 레이어 타일 1장의 가로 길이
        [HideInInspector] public Vector3 startPos;
    }

    [Header("Global Speed")]
    public float baseSpeed = 2f;

    [Header("Layers (set in Inspector)")]
    public Layer[] layers;

    void Start()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].root != null)
                layers[i].startPos = layers[i].root.position;
        }
    }

    void Update()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            var l = layers[i];
            if (l.root == null) continue;

            float layerSpeed = baseSpeed * l.multiplier;
            float offset = Mathf.Repeat(Time.time * layerSpeed, l.repeatWidth);
            l.root.position = l.startPos + Vector3.left * offset;
        }
    }
}