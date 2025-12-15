using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SkullProjectile : MonoBehaviour
{
    [Header("Damage")]
    public float lifeTime = 6f;
    public int damage = 1;

    [Header("Targeting")]
    public bool autoFindTarget = true;
    public string targetTag = "Player";
    public bool aimAtColliderCenter = true;
    public float aimHeight = 1.2f;

    [Header("Homing")]
    public bool homing = true;
    public float turnSpeedDeg = 540f;
    public float homingStartDelay = 0.05f;
    public float homingTime = 3.0f;

    Rigidbody rb;
    Transform target;
    Collider targetCol;

    float speed;
    float spawnTime;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void OnEnable()
    {
        spawnTime = Time.time;
        if (autoFindTarget && target == null) AcquireTarget();
    }

    void AcquireTarget()
    {
        var p = GameObject.FindGameObjectWithTag(targetTag);
        if (!p) return;

        target = p.transform;

        if (aimAtColliderCenter)
        {
            targetCol = p.GetComponent<Collider>();
            if (!targetCol) targetCol = p.GetComponentInChildren<Collider>();
            if (!targetCol) targetCol = p.GetComponentInParent<Collider>();
        }
    }

    Vector3 GetTargetPos()
    {
        if (targetCol) return targetCol.bounds.center;
        if (target) return target.position + Vector3.up * aimHeight;
        return transform.position + transform.forward * 2f;
    }

    // old call
    public void Init(Vector3 velocity)
    {
        speed = Mathf.Max(0.01f, velocity.magnitude);
        rb.velocity = velocity;
        Destroy(gameObject, lifeTime);
    }

    // your BossSkullMinion call (2 params) -> now supported
    public void Init(Vector3 velocity, Transform owner)
    {
        Init(velocity);
        if (owner) IgnoreOwnerCollisions(owner);
    }

    void IgnoreOwnerCollisions(Transform owner)
    {
        var myCol = GetComponent<Collider>();
        if (!myCol) return;

        var cols = owner.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols)
        {
            if (c && !c.isTrigger)
                Physics.IgnoreCollision(myCol, c, true);
        }
    }

    public void SetTarget(Transform t)
    {
        target = t;
        targetCol = null;

        if (t && aimAtColliderCenter)
        {
            targetCol = t.GetComponent<Collider>();
            if (!targetCol) targetCol = t.GetComponentInChildren<Collider>();
            if (!targetCol) targetCol = t.GetComponentInParent<Collider>();
        }
    }

    void FixedUpdate()
    {
        if (!homing) return;

        if (autoFindTarget && target == null) AcquireTarget();
        if (target == null) return;

        float t = Time.time - spawnTime;
        if (t < homingStartDelay) return;
        if (t > homingTime) return;

        Vector3 to = GetTargetPos() - rb.position;
        if (to.sqrMagnitude < 0.0001f) return;

        Vector3 desiredVel = to.normalized * speed;
        Vector3 currentVel = rb.velocity.sqrMagnitude < 0.001f ? desiredVel : rb.velocity;

        float maxRad = Mathf.Deg2Rad * turnSpeedDeg * Time.fixedDeltaTime;
        Vector3 newVel = Vector3.RotateTowards(currentVel, desiredVel, maxRad, 0f).normalized * speed;

        rb.velocity = newVel;

        if (rb.velocity.sqrMagnitude > 0.01f)
            transform.forward = rb.velocity.normalized;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        Transform root = other.transform.root;
        bool isPlayer = other.CompareTag(targetTag) || (root != null && root.CompareTag(targetTag));

        if (isPlayer)
        {
            Health h = other.GetComponent<Health>();
            if (!h && root) h = root.GetComponent<Health>();
            if (!h) h = other.GetComponentInParent<Health>();
            if (!h && root) h = root.GetComponentInChildren<Health>();

            if (h)
            {
                h.TakeDamage(damage);

                // camera shake on hit (same style as EnemyAI)
                if (CameraShake.Instance != null)
                    CameraShake.Instance.Shake();
            }

            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}

