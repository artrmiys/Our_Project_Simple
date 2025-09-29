using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineGlow : MonoBehaviour
{
    public Color glowColor = Color.cyan;
    [Range(0f, 10f)] public float intensity = 5f;

    void Start()
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr.material != null)
        {
            lr.material.EnableKeyword("_EMISSION");
            lr.material.SetColor("_EmissionColor", glowColor * intensity);
        }
    }
}
