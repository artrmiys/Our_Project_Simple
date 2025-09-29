using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GlowAlways : MonoBehaviour
{
    [Header("glow settings")]
    public Color glowColor = Color.cyan;
    public float intensity = 2f;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // clone material (чтобы не трогать общий)
            Material mat = rend.material;

            // включаем эмиссию
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            mat.SetColor("_EmissionColor", glowColor * intensity);
        }
    }
}
