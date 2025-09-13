using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class RopeFromHabrador : MonoBehaviour
{
    [SerializeField] private Material ropeMaterial;

    [Header("Точки")]
    public Transform startPoint;              
    public Transform endPoint;                

    [Header("Геометрия каната")]
    [Range(4, 64)] public int segments = 40;   
    public float slack = 0.1f;                // слабина (0 = натянуто, 0.2 = лёгкое провисание)
    public float thickness = 0.02f;          // толщина линии
    public Color color = Color.red;

    [Header("Движение")]
    public float damping = 0.92f;            // затухание (0.99–0.999)
    public float gravity = 1.2f;              // лёгкая «тяжесть» вниз
    public Vector3 wind = new Vector3(0.12f, 0f, 0.08f); // ветер
    [Range(1, 6)] public int constraintIterations = 2;  // итерации «подтяжки» длины
    [Range(0f, 1f)] public float stiffness = 0.3f;      // жёсткость constraints

    // runtime
    struct Node { public Vector3 pos, prev; }
    List<Node> nodes;
    LineRenderer lr;

    void OnEnable() => Build();
    void OnValidate() { if (Application.isPlaying == false) Build(); }
    void Update()
    {
        if (!startPoint || !endPoint || nodes == null) return;
        float dt = Application.isPlaying ? Time.deltaTime : 1f / 60f;
        Simulate(dt);
        Render();
    }

    // create ropes
    void Build()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;

        if (ropeMaterial != null)
        {
            lr.sharedMaterial = ropeMaterial;
        }
        else
        {
            // fallback на дефолтный шейдер, если забыли задать материал в инспекторе
            Shader sh = Shader.Find("Sprites/Default");
            if (!sh) sh = Shader.Find("Universal Render Pipeline/Unlit");
            lr.sharedMaterial = new Material(sh);
        }

        lr.startColor = lr.endColor = color;
        lr.startWidth = lr.endWidth = thickness;
        lr.numCornerVertices = 2; lr.numCapVertices = 2;
        lr.textureMode = LineTextureMode.Stretch;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        if (!startPoint || !endPoint) return;

        nodes = new List<Node>(segments);
        Vector3 A = startPoint.position;
        Vector3 B = endPoint.position;
        float baseLen = Vector3.Distance(A, B) * (1f + slack);
        float straightStep = Vector3.Distance(A, B) / (segments - 1);
        Vector3 dir = (B - A).normalized;

        for (int i = 0; i < segments; i++)
        {
            // dot throw 
            Vector3 p = A + dir * (straightStep * i);
            // light stretch
            float u = i / (float)(segments - 1);
            float sag = Mathf.Sin(u * Mathf.PI) * baseLen * 0.1f * slack;
            p += Vector3.down * sag;

            nodes.Add(new Node { pos = p, prev = p });
        }
    }

    void Simulate(float dt)
    {
        if (nodes == null || nodes.Count == 0) return;

        Vector3 A = startPoint.position;
        Vector3 B = endPoint.position;

        // length between segments
        float segLen = Vector3.Distance(A, B) * (1f + slack) / (segments - 1);

        // hang position
        nodes[0] = new Node { pos = A, prev = A };
        nodes[^1] = new Node { pos = B, prev = B };

        // inside dots
        for (int i = 1; i < nodes.Count - 1; i++)
        {
            var n = nodes[i];
            Vector3 x = n.pos;
            Vector3 v = (n.pos - n.prev) * damping;

            Vector3 F = Vector3.down * gravity + wind;   // простые силы
            Vector3 xNew = x + v + F * (dt * dt);

            n.prev = n.pos;
            n.pos = xNew;
            // check ground
            if (Physics.Raycast(n.pos + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 1f))
            {
                if (n.pos.y < hit.point.y)
                    n.pos.y = hit.point.y;
            }


            nodes[i] = n;
        }

        // positional stop
        for (int it = 0; it < constraintIterations; it++)
        {
            // keep ends
            nodes[0] = new Node { pos = A, prev = A };
            nodes[^1] = new Node { pos = B, prev = B };

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                var nA = nodes[i];
                var nB = nodes[i + 1];

                Vector3 delta = nB.pos - nA.pos;
                float dist = delta.magnitude;
                if (dist < 1e-6f) continue;

                float diff = (dist - segLen) / dist;
                Vector3 corr = delta * (0.5f * diff * stiffness);

                // stay ends
                if (i == 0)
                {
                    nB.pos -= delta * (diff * stiffness);
                }
                else if (i == nodes.Count - 2)
                {
                    nA.pos += delta * (diff * stiffness);
                }
                else
                {
                    nA.pos += corr;
                    nB.pos -= corr;
                }

                nodes[i] = nA;
                nodes[i + 1] = nB;
            }
        }
    }

    void Render()
    {
        lr.positionCount = nodes.Count;
        for (int i = 0; i < nodes.Count; i++) lr.SetPosition(i, nodes[i].pos);
    }
}
