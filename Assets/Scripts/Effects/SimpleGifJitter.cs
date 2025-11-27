using UnityEngine;
using UnityEngine.UI;

public class SimpleGifJitter : MonoBehaviour
{
    [Header("Frames")]
    public Texture[] frames;      // 3 textures
    public float frameTime = 0.15f;

    [Header("Jitter (UV)")]
    public float uvJitter = 0.005f; // small offset for old-film feel

    private MeshRenderer mr;
    private Material mat;
    private int currentFrame;
    private float timer;
    private Vector2 baseOffset;

    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mat = mr.material;                 // instance
            baseOffset = mat.mainTextureOffset;
        }
    }

    private void OnEnable()
    {
        timer = 0f;
        currentFrame = 0;

        if (mat != null && frames != null && frames.Length > 0)
            mat.mainTexture = frames[0];
    }

    private void Update()
    {
        if (mat == null || frames == null || frames.Length == 0)
            return;

        // frame switch
        timer += Time.deltaTime;
        if (timer >= frameTime)
        {
            timer -= frameTime;
            currentFrame++;
            if (currentFrame >= frames.Length)
                currentFrame = 0;

            mat.mainTexture = frames[currentFrame];
        }

        // UV jitter
        Vector2 jitter = new Vector2(
            Random.Range(-uvJitter, uvJitter),
            Random.Range(-uvJitter, uvJitter)
        );
        mat.mainTextureOffset = baseOffset + jitter;
    }
}