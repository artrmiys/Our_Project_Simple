using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeFromHabrador : MonoBehaviour
{
    [SerializeField] private Material ropeMaterial;

    [Header("Привязка")]
    public Transform startPoint;              // крепление к персонажу
    public float length = 1f;                 // длина нити
    [Range(4, 64)] public int segments = 30;  // количество сегментов
    public Transform player;                  // центр персонажа

    [Header("Математическая капсула")]
    public float capsuleHeight = 3.8f;        // высота
    public float capsuleRadius = 2.4f;        // радиус

    [Header("Пол")]
    public float groundY = 2f;                // уровень пола

    [Header("Визуал")]
    public float thickness = 0.02f;
    public Color color = Color.white;

    [Header("Физика")]
    public float damping = 0.98f;
    public float gravity = 0.02f;
    public float swayStrength = 0.05f;
    public float stiffness = 0.9f;
    public int constraintIterations = 4;

    struct Node { public Vector3 pos, prev; }
    List<Node> nodes;
    LineRenderer lr;
    float segLen;

    void OnEnable()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.startWidth = lr.endWidth = thickness;
        lr.startColor = lr.endColor = color;

        if (ropeMaterial != null)
            lr.sharedMaterial = ropeMaterial;
        else
            lr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

        Build();
    }

    void Build()
    {
        if (!startPoint) return;

        nodes = new List<Node>(segments);
        Vector3 start = startPoint.position;

        segLen = length / (segments - 1);

        for (int i = 0; i < segments; i++)
        {
            Vector3 p = start + Vector3.down * segLen * i;
            nodes.Add(new Node { pos = p, prev = p });
        }
    }

    void Update()
    {
        if (nodes == null || nodes.Count == 0) return;
        Simulate(Time.deltaTime);
        Render();
    }

    void Simulate(float dt)
    {
        // фиксируем верх
        nodes[0] = new Node { pos = startPoint.position, prev = startPoint.position };

        for (int i = 1; i < nodes.Count; i++)
        {
            var n = nodes[i];
            Vector3 vel = (n.pos - n.prev) * damping;

            // силы
            Vector3 F = Vector3.down * gravity;
            F += new Vector3(
                Mathf.Sin(Time.time * 1.3f + i) * swayStrength,
                0,
                Mathf.Cos(Time.time * 0.7f + i) * swayStrength
            );

            Vector3 newPos = n.pos + vel * dt + F * (dt * dt * 0.99f);

            n.prev = n.pos;
            n.pos = newPos;

            // === Пол ===
            if (n.pos.y < groundY)
                n.pos.y = groundY + 0.01f;

            // === Капсула персонажа ===
            if (player != null)
            {
                Vector3 p1 = player.position; // нижняя точка
                Vector3 p2 = player.position + Vector3.up * capsuleHeight; // верхняя точка

                // проекция точки на ось капсулы
                Vector3 axis = p2 - p1;
                float t = Vector3.Dot(n.pos - p1, axis.normalized);
                t = Mathf.Clamp(t, 0, axis.magnitude);

                Vector3 closest = p1 + axis.normalized * t;
                Vector3 dir = n.pos - closest;
                float dist = dir.magnitude;

                if (dist < capsuleRadius)
                {
                    dir = dir.normalized * capsuleRadius;
                    n.pos = closest + dir;
                }
            }

            nodes[i] = n;
        }

        // === Constraints (длина) ===
        for (int it = 0; it < constraintIterations; it++)
        {
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                var nA = nodes[i];
                var nB = nodes[i + 1];

                Vector3 delta = nB.pos - nA.pos;
                float dist = delta.magnitude;
                if (dist < 1e-6f) continue;

                float diff = (dist - segLen) / dist;
                Vector3 corr = delta * (0.5f * diff * stiffness);

                if (i == 0)
                    nB.pos -= delta * (diff * stiffness);
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
        for (int i = 0; i < nodes.Count; i++)
            lr.SetPosition(i, nodes[i].pos);
    }
}
