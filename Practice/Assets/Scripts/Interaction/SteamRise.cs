using UnityEngine;

public class SteamRiseFadeMesh : MonoBehaviour
{
    [Header("Timing")]
    public float cycleDuration = 4f; // seconds for full rise
    [Range(0f, 1f)]
    public float cycleOffset = 0f;

    [Header("Movement")]
    public float maxHeight = 1.2f;

    [Header("Fade")]
    public float fadeInHeight = 0.2f;
    public float fadeOutHeight = 0.3f;

    private Vector3 startPos;
    private float timer;

    private Renderer rend;
    private MaterialPropertyBlock propBlock;
    private Color baseColor;

    void Start()
    {
        startPos = transform.localPosition;
        timer = cycleOffset * cycleDuration;

        rend = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
        baseColor = rend.sharedMaterial.color;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > cycleDuration)
            timer -= cycleDuration;

        float t = timer / cycleDuration; // 0 → 1
        float height = t * maxHeight;

        transform.localPosition = startPos + Vector3.up * height;

        float alpha = 1f;

        if (height < fadeInHeight)
            alpha = Mathf.InverseLerp(0f, fadeInHeight, height);
        else if (height > maxHeight - fadeOutHeight)
            alpha = Mathf.InverseLerp(maxHeight, maxHeight - fadeOutHeight, height);

        Color c = baseColor;
        c.a = alpha;

        propBlock.SetColor("_Color", c);
        rend.SetPropertyBlock(propBlock);
    }
}
