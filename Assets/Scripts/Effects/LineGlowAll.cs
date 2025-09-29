using UnityEngine;

public class LineGlowAll : MonoBehaviour
{
    [Header("glow settings")]
    public Color glowColor = Color.cyan;
    [Range(0f, 10f)] public float intensity = 5f;

    void Start()
    {
        // ������� ��� LineRenderer � �����
        LineRenderer[] lines = FindObjectsOfType<LineRenderer>();

        foreach (LineRenderer lr in lines)
        {
            if (lr.material != null)
            {
                // ��������� ��������, ����� �� ������� sharedMaterial
                Material mat = new Material(lr.material);

                // �������� �������
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                mat.SetColor("_EmissionColor", glowColor * intensity);

                // ��������� ����� ��������
                lr.material = mat;
            }
        }
    }
}

