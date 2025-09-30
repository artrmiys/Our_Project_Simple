using UnityEngine;

public class MaterialGlowSingle : MonoBehaviour
{
    [Header("glow settings")]
    public Material targetMaterial;  // материал
    public Color glowColor = Color.cyan;
    [Range(0f, 10f)] public float intensity = 5f;

    void Start()
    {
        if (targetMaterial != null)
        {
            targetMaterial.EnableKeyword("_EMISSION");
            targetMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            Color finalColor = glowColor * Mathf.LinearToGammaSpace(intensity);
            targetMaterial.SetColor("_EmissionColor", finalColor);
        }
    }
}
