using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class TMPGlow : MonoBehaviour
{
    [Range(0f, 1f)] public float outlineWidth = 0.12f;
    [Range(0f, 5f)] public float glowPower = 0.6f;

    private TextMeshPro tmp;
    private Material runtimeMat;   // наша отдельная копия

    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();

        // делаем копию исходного материала, чтобы не сломать общий
        runtimeMat = Instantiate(tmp.fontSharedMaterial);
        tmp.fontMaterial = runtimeMat;

        ApplyFromCurrentColor();
    }

    void OnEnable()
    {
        ApplyFromCurrentColor();
    }

    void ApplyFromCurrentColor()
    {
        if (tmp == null || runtimeMat == null) return;

        // берём тот цвет, который уже поставили другие скрипты (красный/зелёный)
        Color c = tmp.color;

        // outline, если есть
        if (runtimeMat.HasProperty("_OutlineColor"))
        {
            runtimeMat.SetColor("_OutlineColor", c);
            runtimeMat.SetFloat("_OutlineWidth", outlineWidth);
        }

        // glow, если есть в шейдере
        if (runtimeMat.HasProperty("_GlowColor"))
            runtimeMat.SetColor("_GlowColor", c);
        if (runtimeMat.HasProperty("_GlowPower"))
            runtimeMat.SetFloat("_GlowPower", glowPower);
    }
}
