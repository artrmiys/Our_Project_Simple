using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rope_Follow_WithCollision : MonoBehaviour
{
    [Header("Anchors")]
    public Transform startPoint;                 // верхняя точка
    public Transform endPoint;                   // точка на игроке/кости

    [Header("Rope")]
    [Range(2, 60)] public int segments = 16;
    public float ropeLength = 3f;
    public float massPerSegment = 0.05f;
    public float drag = 0.05f;
    public float angularDrag = 0.2f;

    [Header("Collision (optional)")]
    public bool enableCollision = true;          // включить коллизии с миром
    public float colliderRadius = 0.02f;         // «толщина» физики
    public LayerMask worldLayers = ~0;           // чем можно сталкиваться (по умолчанию всё)
    public string ropeLayerName = "Rope";        // слой сегментов (создай его в проекте)
    public string playerLayerName = "Player";    // слой игрока (чтобы игнорить столкновения)

    [Header("Joint limits")]
    [Range(5, 45)] public float angularLimit = 20f;
    public float projectionDistance = 0.02f;
    public float projectionAngle = 15f;

    [Header("Render")]
    public float lineWidth = 0.02f;
    public Color lineColor = Color.red;

    // runtime
    private readonly List<Transform> nodes = new();
    private float segLen;
    private LineRenderer line;
    private ConfigurableJoint endJoint;          // конец к мировой точке
    private Rigidbody startRB;
    private int ropeLayer = -1, playerLayer = -1;

    void Awake()
    {
        transform.localScale = Vector3.one;

        // Слои (если не найдены — останется Default)
        ropeLayer = LayerMask.NameToLayer(ropeLayerName);
        playerLayer = LayerMask.NameToLayer(playerLayerName);

        // Глобально игнорируем Rope vs Player (на всякий случай)
        if (ropeLayer >= 0 && playerLayer >= 0)
            Physics.IgnoreLayerCollision(ropeLayer, playerLayer, true);

        // Настройка LineRenderer
        line = GetComponent<LineRenderer>();
        if (!line.material)
        {
            var sh = Shader.Find("Sprites/Default");
            if (!sh) sh = Shader.Find("Universal Render Pipeline/Unlit");
            line.material = new Material(sh);
            if (line.material.HasProperty("_Surface")) line.material.SetFloat("_Surface", 1f);
            line.material.renderQueue = 3000;
        }
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.startColor = lineColor;
        line.endColor = lineColor;
        line.alignment = LineAlignment.View;
        line.textureMode = LineTextureMode.Stretch;
        line.numCornerVertices = 2;
        line.numCapVertices = 2;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.useWorldSpace = true;
    }

    void Start() => Build();

    void FixedUpdate()
    {
        // держим конец в мировой точке кости/крюка
        if (endJoint && endPoint)
            endJoint.connectedAnchor = endPoint.position;
    }

    void LateUpdate()
    {
        if (!line || nodes.Count == 0 || !startPoint || !endPoint) return;

        line.positionCount = nodes.Count + 2;
        line.SetPosition(0, startPoint.position);
        for (int i = 0; i < nodes.Count; i++) line.SetPosition(i + 1, nodes[i].position);
        line.SetPosition(nodes.Count + 1, endPoint.position);
    }

    public void Build()
    {
        foreach (var t in nodes) if (t) Destroy(t.gameObject);
        nodes.Clear();
        if (!startPoint || !endPoint) return;

        // якорь-ригид — кинематический
        startRB = startPoint.GetComponent<Rigidbody>();
        if (!startRB) startRB = startPoint.gameObject.AddComponent<Rigidbody>();
        startRB.isKinematic = true;
        startRB.interpolation = RigidbodyInterpolation.Interpolate;

        segLen = ropeLength / Mathf.Max(2, segments);

        Rigidbody prev = startRB;
        for (int i = 0; i < segments; i++)
        {
            var seg = CreateSegment($"RopeSeg_{i}");
            float t = (i + 1f) / (segments + 1f);
            seg.position = Vector3.Lerp(startPoint.position, endPoint.position, t);

            var rb = seg.GetComponent<Rigidbody>();
            var j = seg.gameObject.AddComponent<ConfigurableJoint>();
            SetupJoint(j, rb, prev);

            nodes.Add(seg);
            prev = rb;
        }

        // конец к мировой точке (без connectedBody)
        endJoint = nodes[^1].gameObject.AddComponent<ConfigurableJoint>();
        endJoint.connectedBody = null;
        endJoint.autoConfigureConnectedAnchor = false;
        endJoint.connectedAnchor = endPoint.position;
        SetupAngular(endJoint);
        endJoint.projectionMode = JointProjectionMode.PositionAndRotation;
        endJoint.projectionDistance = projectionDistance;
        endJoint.projectionAngle = projectionAngle;
    }

    // === helpers ===
    Transform CreateSegment(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, worldPositionStays: true);
        if (ropeLayer >= 0) go.layer = ropeLayer;

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = massPerSegment;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = enableCollision ? CollisionDetectionMode.Continuous : CollisionDetectionMode.Discrete;
        rb.solverIterations = 12;
        rb.solverVelocityIterations = 12;

        if (enableCollision)
        {
            // Узкий CapsuleCollider по оси Y длиной сегмента
            var col = go.AddComponent<CapsuleCollider>();
            col.direction = 1;                // Y
            col.radius = Mathf.Max(0.001f, colliderRadius);
            col.height = Mathf.Max(col.radius * 2.1f, segLen); // минимум > 2*radius
            col.contactOffset = 0.001f;
            // Ограничим столкновения только с нужными слоями
            var filter = go.AddComponent<RigidbodyExtensionsLayerFilter>();
            filter.allowedLayers = worldLayers;
        }

        return go.transform;
    }

    void SetupJoint(ConfigurableJoint j, Rigidbody child, Rigidbody parent)
    {
        j.connectedBody = parent;
        j.autoConfigureConnectedAnchor = true;

        j.xMotion = ConfigurableJointMotion.Locked;
        j.yMotion = ConfigurableJointMotion.Locked;
        j.zMotion = ConfigurableJointMotion.Locked;

        j.projectionMode = JointProjectionMode.PositionAndRotation;
        j.projectionDistance = projectionDistance;
        j.projectionAngle = projectionAngle;

        SetupAngular(j);
    }

    void SetupAngular(ConfigurableJoint j)
    {
        j.angularXMotion = ConfigurableJointMotion.Limited;
        j.angularYMotion = ConfigurableJointMotion.Limited;
        j.angularZMotion = ConfigurableJointMotion.Limited;

        float ang = Mathf.Clamp(angularLimit, 5f, 45f);
        j.lowAngularXLimit = new SoftJointLimit { limit = -ang };
        j.highAngularXLimit = new SoftJointLimit { limit = ang };
        j.angularYLimit = new SoftJointLimit { limit = ang };
        j.angularZLimit = new SoftJointLimit { limit = ang };

        var drive0 = new JointDrive { positionSpring = 0f, positionDamper = 0f, maximumForce = 0f };
        j.xDrive = j.yDrive = j.zDrive = drive0;
        j.angularXDrive = j.angularYZDrive = drive0;
        j.enablePreprocessing = true;
        j.massScale = 1f;
        j.connectedMassScale = 1f;
    }
}

/// <summary>
/// Простой фильтр: разрешает контакты только с указанными слоями.
/// Вешается автоматически на сегмент при включённых коллизиях.
/// </summary>
public class RigidbodyExtensionsLayerFilter : MonoBehaviour
{
    public LayerMask allowedLayers = ~0;
    void OnCollisionEnter(Collision c)
    {
        if ((allowedLayers.value & (1 << c.gameObject.layer)) == 0)
            Physics.IgnoreCollision(
                GetComponent<Collider>(), c.collider, true
            );
    }
}
