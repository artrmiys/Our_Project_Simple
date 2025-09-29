using UnityEngine;

public class LineGlowAll : MonoBehaviour
{
    [Header("glow settings")]
    public Color glowColor = Color.cyan;
    [Range(0f, 10f)] public float intensity = 5f;

    void Start()
    {
        // находим все LineRenderer в сцене
        LineRenderer[] lines = FindObjectsOfType<LineRenderer>();

        foreach (LineRenderer lr in lines)
        {
            if (lr.material != null)
            {
                // дублируем материал, чтобы не портить sharedMaterial
                Material mat = new Material(lr.material);

                // включаем эмиссию
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                mat.SetColor("_EmissionColor", glowColor * intensity);

                // применяем новый материал
                lr.material = mat;
            }
        }
    }
}

