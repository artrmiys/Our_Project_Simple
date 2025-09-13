using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class StrandsOverHeadSky : MonoBehaviour
{
    [Header("Attach")]
    public Transform head;                         // корень на голове
    public Vector3 localOffset = new Vector3(0, 0.15f, 0);

    [Header("Strands")]
    [Range(1, 64)] public int strandCount = 10;
    [Range(4, 48)] public int segments = 16;
    public float strandLength = 1.5f;             // ƒЋ»ЌЌџ≈ нитки (1Ц3 м)
    public float spread = 0.08f;                  // разброс корней
    public float thicknessRoot = 0.004f;
    public float thicknessTip = 0.0007f;
    public Color color = new Color(1f, 0.2f, 0.2f, 1f);

    [Header("Forces")]
    public Vector3 preferredDir = Vector3.up;      // куда Ђв небої
    public float skyPull = 5.0f;                   // т€га к небу (увеличь дл€ более вертикальных)
    public float gravity = 0.0f;                   // обычна€ гравитаци€ вниз (оставь 0)
    public float damping = 0.997f;                 // затухание (1 Ч без)
    public float windStrength = 0.6f;
    public float windNoiseScale = 1.6f;
    public float windNoiseSpeed = 1.1f;

    [Header("Shape control")]
    [Range(0f, 1f)] public float stiffness = 0.6f;  // жЄсткость ограничений длины
    [Range(0f, 1f)] public float straighten = 0.35f;// Ђвыпр€млениеї вдоль preferredDir

    [Header("Rendering")]
    public Material lineMaterial; // создадим Unlit, если пусто

    // --- runtime ---
    class Strand
    {
        public Vector3[] p;        // текущие точки
        public Vector3[] prev;     // предыдущие (Verlet)
        public LineRenderer lr;
        public Vector3 rootLocal;  // лок.корень
    }
    readonly List<Strand> strands = new();

    void OnEnable() => Rebuild();
    void OnValidate() => Rebuild();

    void Update()
    {
        if (!head || strands.Count == 0) return;
        float dt = Application.isPlaying ? Time.deltaTime : 1f / 60f;
        Simulate(dt);
        Render();
    }

    // -------- Build --------
    void Rebuild()
    {
        foreach (var s in strands)
            if (s.lr) { if (Application.isEditor) DestroyImmediate(s.lr.gameObject); else Destroy(s.lr.gameObject); }
        strands.Clear();
        if (!head) return;

        if (!lineMaterial)
        {
            var sh = Shader.Find("Sprites/Default");
            if (!sh) sh = Shader.Find("Universal Render Pipeline/Unlit");
            lineMaterial = new Material(sh);
            if (lineMaterial.HasProperty("_Surface")) lineMaterial.SetFloat("_Surface", 1f);
            lineMaterial.renderQueue = 3000;
            lineMaterial.color = color;
        }

        for (int i = 0; i < strandCount; i++)
        {
            var go = new GameObject($"strand_{i}");
            go.transform.SetParent(transform, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.material = lineMaterial;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.numCornerVertices = 2; lr.numCapVertices = 2;
            lr.textureMode = LineTextureMode.Stretch;

            var curve = new AnimationCurve();
            curve.AddKey(0f, thicknessRoot);
            curve.AddKey(1f, thicknessTip);
            lr.widthCurve = curve;
            lr.startColor = lr.endColor = color;

            var s = new Strand
            {
                p = new Vector3[segments],
                prev = new Vector3[segments],
                lr = lr,
                rootLocal = localOffset + Random.insideUnitSphere * spread
            };

            Vector3 rootW = head.TransformPoint(s.rootLocal);
            Vector3 dir = preferredDir.normalized;
            float step = strandLength / (segments - 1);
            for (int j = 0; j < segments; j++)
            {
                // сразу выт€нем вдоль preferredDir (вверх)
                var pos = rootW + dir * (step * j);
                s.p[j] = s.prev[j] = pos;
            }
            strands.Add(s);
        }
    }

    // -------- Sim --------
    void Simulate(float dt)
    {
        float segLen = strandLength / (segments - 1);
        Vector3 dir = preferredDir.normalized;
        float t = Time.time * windNoiseSpeed;

        for (int i = 0; i < strands.Count; i++)
        {
            var s = strands[i];

            // жЄстко закрепл€ем корень
            Vector3 rootW = head.TransformPoint(s.rootLocal);
            s.p[0] = s.prev[0] = rootW;

            // интеграци€
            for (int j = 1; j < segments; j++)
            {
                Vector3 x = s.p[j];
                Vector3 xPrev = s.prev[j];

                // базовые силы: Ђнебої вверх + немного обычной гравитации вниз
                Vector3 F = dir * skyPull + Vector3.down * gravity;

                // ветер (плавный шум)
                float n = Mathf.PerlinNoise(i * 0.37f + j * 0.21f + t, i * 0.11f - j * 0.23f + t);
                Vector3 wind = new Vector3(n - 0.5f, (0.5f - n) * 0.3f, Mathf.Sin(n * 6.283f)) * windStrength;
                F += wind;

                Vector3 xNew = x + (x - xPrev) * damping + F * (dt * dt);
                s.prev[j] = x;
                s.p[j] = xNew;
            }

            // 1) ограничени€ длины (несколько итераций Ч жЄстче)
            for (int it = 0; it < 3; it++)
            {
                s.p[0] = head.TransformPoint(s.rootLocal); // корень
                for (int j = 0; j < segments - 1; j++)
                {
                    Vector3 a = s.p[j];
                    Vector3 b = s.p[j + 1];
                    Vector3 ab = b - a;
                    float dist = ab.magnitude; if (dist < 1e-6f) continue;
                    float diff = (dist - segLen) / dist;

                    if (j == 0)
                    {
                        s.p[j + 1] -= ab * diff * stiffness;     // не двигаем корень
                    }
                    else
                    {
                        Vector3 corr = ab * (0.5f * diff * stiffness);
                        s.p[j] += corr;
                        s.p[j + 1] -= corr;
                    }
                }
            }

            // 2) Ђ¬ыпр€мительї: т€нем точки к идеальной позиции вдоль preferredDir
            //    (держит шлейф вертикальным/небесным и очень длинным)
            for (int j = 1; j < segments; j++)
            {
                Vector3 ideal = s.p[0] + dir * (segLen * j);
                s.p[j] = Vector3.Lerp(s.p[j], ideal, straighten);
            }
        }
    }

    // -------- Render --------
    void Render()
    {
        foreach (var s in strands)
        {
            s.lr.positionCount = segments;
            s.lr.SetPositions(s.p);
        }
    }
}
