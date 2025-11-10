using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WhipWide : MonoBehaviour
{
    [Header("Arc")]
    public float startRadius = 0.3f;
    public float endRadius = 2.5f;
    public float totalAngle = 300f;
    public int segments = 24;

    [Header("Build")]
    public float buildTime = 0.12f;   // how fast line grows
    float buildT = 0f;

    [Header("Wave")]
    public float waveSize = 0.35f;
    public float waveSpeed = 20f;

    [Header("Lifetime")]
    public float lifetime = 0.35f;

    [Header("Damage")]
    public int damage = 2;
    public float hitRadius = 0.25f;
    public float hitCooldown = 0.15f;

    [Header("Visual")]
    public float thickness = 0.06f;
    public Color color = Color.red;
    public GameObject flashPrefab;
    public float flashHitScale = 2f;

    LineRenderer lr;
    Vector3[] points;
    float[] yNoise;                  // fixed noise per point
    GameObject flashInstance;
    Vector3 flashDefaultScale = Vector3.one;

    Dictionary<Health, float> lastHit = new Dictionary<Health, float>();
    Transform owner;

    public void SetOwner(Transform t) => owner = t;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = segments;
        lr.useWorldSpace = true;
        lr.startWidth = thickness;
        lr.endWidth = thickness * 0.15f;
        lr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = color;

        points = new Vector3[segments];
        yNoise = new float[segments];
        for (int i = 0; i < segments; i++)
            yNoise[i] = Random.Range(-0.15f, 0.15f);   // fixed vertical wobble

        if (flashPrefab)
        {
            flashInstance = Instantiate(flashPrefab, transform.position, Quaternion.identity);
            flashDefaultScale = flashInstance.transform.localScale;
        }

        StartCoroutine(DestroyAfter(lifetime));
    }

    void Update()
    {
        // grow 0..1
        if (buildTime > 0f)
            buildT = Mathf.Clamp01(buildT + Time.deltaTime / buildTime);
        else
            buildT = 1f;

        BuildArcFacingCamera(buildT);
        bool hit = DoHits(buildT);

        if (flashInstance)
        {
            flashInstance.transform.position = transform.position;
            if (hit)
                flashInstance.transform.localScale = flashDefaultScale * flashHitScale;
        }
    }

    void BuildArcFacingCamera(float grow)
    {
        Vector3 center = transform.position;

        // direction from camera
        Vector3 viewFwd = Camera.main ? Camera.main.transform.forward : transform.forward;

        // horizontal forward
        Vector3 flatFwd = new Vector3(viewFwd.x, 0f, viewFwd.z);
        if (flatFwd.sqrMagnitude < 0.0001f)
            flatFwd = transform.forward;
        flatFwd.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, flatFwd).normalized;

        // small tilt by camera pitch
        float tilt = Mathf.Clamp(viewFwd.y, -0.6f, 0.6f);
        Vector3 upTilt = Vector3.up + Vector3.up * tilt * 0.5f;

        float angleStep = totalAngle / (segments - 1);

        for (int i = 0; i < segments; i++)
        {
            // how far along full arc this point is
            float pointT = i / (float)(segments - 1);

            // limit by build progress
            if (pointT > grow)
            {
                // keep tail at center so line "grows"
                points[i] = center;
                continue;
            }

            float currRadius = Mathf.Lerp(startRadius, endRadius, pointT);
            float angDeg = -totalAngle * 0.5f + angleStep * i;
            float angRad = angDeg * Mathf.Deg2Rad;

            // base dir on XZ
            Vector3 dir = (right * Mathf.Cos(angRad)) + (flatFwd * Mathf.Sin(angRad));
            dir.Normalize();

            Vector3 pos = center + dir * currRadius;

            // wave like in WhipSimple (stronger to the end)
            float wave = Mathf.Sin(Time.time * waveSpeed - i * 0.4f) * waveSize * pointT;
            Vector3 side = Vector3.Cross(dir, Vector3.up).normalized;
            pos += side * wave;

            // fixed small vertical
            pos += upTilt * yNoise[i];

            points[i] = pos;
        }

        lr.SetPositions(points);
    }

    bool DoHits(float grow)
    {
        bool hitSomething = false;

        for (int i = 0; i < segments; i++)
        {
            float pointT = i / (float)(segments - 1);
            if (pointT > grow) break; // do not hit beyond grown part

            Collider[] cols = Physics.OverlapSphere(points[i], hitRadius);
            foreach (var col in cols)
            {
                if (owner && col.transform.IsChildOf(owner)) continue;

                Health h = col.GetComponent<Health>() ?? col.GetComponentInParent<Health>();
                if (!h) continue;

                if (!lastHit.ContainsKey(h) || Time.time - lastHit[h] > hitCooldown)
                {
                    h.TakeDamage(damage);
                    lastHit[h] = Time.time;
                    hitSomething = true;

                    if (CameraShake.Instance != null)
                        CameraShake.Instance.Shake();
                }
            }
        }

        return hitSomething;
    }

    IEnumerator DestroyAfter(float t)
    {
        yield return new WaitForSeconds(t);
        if (flashInstance) Destroy(flashInstance);
        Destroy(gameObject);
    }
}
