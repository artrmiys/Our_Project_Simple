using UnityEngine;

public class BossSkullMinion : MonoBehaviour
{
    [Header("CENTER (PICK ONE)")]
    public Transform centerAnchor;      // if set -> exact center
    public Collider bossCollider;       // else -> center from collider local center
    public float centerHeightOffset = 0f;

    [Header("VISUAL (PIVOT FIX)")]
    public Transform visual;            // optional mesh child
    public Vector3 visualLocalOffset = Vector3.zero;

    [Header("ORBIT")]
    public float orbitRadius = 1f;
    public float orbitY = 0f;
    public float orbitSpeedDeg = 120f;

    [Header("FOLLOW")]
    public bool strictOrbit = false;
    public float followLerp = 20f;

    [Header("ROTATION")]
    public bool lookOutward = true;
    public float yawOffset = 0f;
    public float lookLerp = 14f;

    [Header("ATTACK")]
    public Transform player;
    public Transform shootPoint;

    [Header("PROJECTILE (NO PREFAB NEEDED)")]
    [Tooltip("Optional: put here a child object to CLONE as projectile (mesh, material, etc). If empty -> runtime sphere.")]
    public Transform projectileTemplate;     // optional (can be your 'projectil' object)

    public int projectileDamage = 1;
    public float projectileLifeTime = 6f;
    public float projectileSpeed = 14f;
    public float shootCooldown = 1.2f;
    public float playerAimHeight = 1.0f;

    [Tooltip("Used only when projectileTemplate is empty (runtime sphere).")]
    public float runtimeProjectileSize = 0.18f;

    [Tooltip("Optional material for runtime sphere projectile.")]
    public Material runtimeProjectileMaterial;

    float angleDeg;
    float nextShootTime;
    bool shotArmed;

    void Awake()
    {
        if (!shootPoint) shootPoint = transform;

        if (visual)
            visual.localPosition = visualLocalOffset;
    }

    void OnValidate()
    {
        if (visual)
            visual.localPosition = visualLocalOffset;
    }

    void LateUpdate()
    {
        Vector3 center = GetCenterWorld();

        angleDeg += orbitSpeedDeg * Time.deltaTime;
        float rad = angleDeg * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * orbitRadius;
        Vector3 targetPos = center + Vector3.up * orbitY + offset;

        if (strictOrbit)
            transform.position = targetPos;
        else
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followLerp);

        Vector3 dir = lookOutward ? (transform.position - center) : (center - transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized) * Quaternion.Euler(0f, yawOffset, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * lookLerp);
        }
    }

    Vector3 GetCenterWorld()
    {
        if (centerAnchor)
            return centerAnchor.position + Vector3.up * centerHeightOffset;

        if (!bossCollider)
            return transform.position;

        if (bossCollider is CapsuleCollider cc)
            return cc.transform.TransformPoint(cc.center) + Vector3.up * centerHeightOffset;

        if (bossCollider is BoxCollider bc)
            return bc.transform.TransformPoint(bc.center) + Vector3.up * centerHeightOffset;

        if (bossCollider is SphereCollider sc)
            return sc.transform.TransformPoint(sc.center) + Vector3.up * centerHeightOffset;

        return bossCollider.transform.position + Vector3.up * centerHeightOffset;
    }

    // BossAI calls this when attack starts
    public void PrepareShot()
    {
        shotArmed = true;
    }

    // Animation Event OR BossAI fallback calls this
    public bool ShootAtPlayer()
    {
        if (!shotArmed) return false;
        if (!player) return false;
        if (Time.time < nextShootTime) return false;

        nextShootTime = Time.time + shootCooldown;
        shotArmed = false;

        Vector3 from = shootPoint ? shootPoint.position : transform.position;
        Vector3 to = player.position + Vector3.up * playerAimHeight;
        Vector3 dir = (to - from).normalized;

        GameObject go = SpawnProjectile(from, dir);

        // ensure projectile logic exists
        SkullProjectile proj = go.GetComponent<SkullProjectile>();
        if (!proj) proj = go.AddComponent<SkullProjectile>();

        proj.damage = projectileDamage;
        proj.lifeTime = projectileLifeTime;

        // ignore collision with boss
        Collider projCol = go.GetComponent<Collider>();
        if (projCol && bossCollider)
            Physics.IgnoreCollision(projCol, bossCollider, true);

        Transform owner = bossCollider ? bossCollider.transform : null;
        proj.Init(dir * projectileSpeed, owner);

        return true;
    }

    GameObject SpawnProjectile(Vector3 pos, Vector3 dir)
    {
        GameObject go;

        // Option A: clone template (если хочешь красивый снаряд)
        if (projectileTemplate)
        {
            go = Instantiate(projectileTemplate.gameObject, pos, Quaternion.LookRotation(dir));
            go.SetActive(true);
        }
        else
        {
            // Option B: create runtime sphere (вообще без префабов)
            go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "SkullProjectile_Runtime";
            go.transform.position = pos;
            go.transform.rotation = Quaternion.LookRotation(dir);
            go.transform.localScale = Vector3.one * runtimeProjectileSize;

            var col = go.GetComponent<Collider>();
            col.isTrigger = true;

            var rb = go.GetComponent<Rigidbody>();
            if (!rb) rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            if (runtimeProjectileMaterial)
            {
                var r = go.GetComponent<Renderer>();
                if (r) r.material = runtimeProjectileMaterial;
            }
        }

        // make sure collider is trigger
        var c = go.GetComponent<Collider>();
        if (c) c.isTrigger = true;

        // make sure rigidbody exists
        var rigid = go.GetComponent<Rigidbody>();
        if (!rigid) rigid = go.AddComponent<Rigidbody>();
        rigid.useGravity = false;
        rigid.isKinematic = false;
        rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigid.constraints = RigidbodyConstraints.FreezeRotation;

        // keep same layer as minion (so physics settings consistent)
        go.layer = gameObject.layer;

        return go;
    }
}


