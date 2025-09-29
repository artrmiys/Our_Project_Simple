using UnityEngine;

public class MaterialsGlow : MonoBehaviour
{
    [Header("glow settings")]
    public Material[] targetMaterials;  // список материалов
    public Color glowColor = Color.cyan;
    [Range(0f, 10f)] public float intensity = 5f;

    void Start()
    {
        foreach (Material mat in targetMaterials)
        {
            if (mat != null)
            {
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                mat.SetColor("_EmissionColor", glowColor * intensity);
            }
        }
    }
}
