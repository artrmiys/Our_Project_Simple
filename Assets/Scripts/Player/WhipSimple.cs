using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WhipSimple : MonoBehaviour
{
    [Header("Whip set")]
    public float length = 5f;       // whip len
    public int segments = 12;       // whip segs
    public float lifetime = 0.4f;   // life sec
    public float waveSize = 0.5f;   // wave amp
    public float waveSpeed = 20f;   // wave spd
    public float thickness = 0.05f; // base width
    public Color color = Color.cyan;
    public int damage = 1;

    [Header("Flash set")]
    public GameObject flashPrefab;
    public float flashHitScale = 2f;

    [Header("Pull by whip hit (NO button)")]
    public bool enablePull = true;
    public string pullableTag = "Pullable";  // назначь этот тег объектам, которые можно тянуть
    public float pullCheckRadius = 0.18f;    // радиус “касания” нити

    private LineRenderer lr;
    private Vector3[] points;
    private bool spawnedFlash = false;
    private GameObject flashInstance;
    private Vector3 flashDefaultScale = Vector3.one;

    // hit cd
    private Dictionary<Health, float> lastHit = new Dictionary<Health, float>();
    private float hitCooldown = 0.2f;

    // pull
    private ThreadPullToPlayer pullSystem;
    private bool pullTriggered = false;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = segments;
        lr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = color;

        // base widths
        lr.startWidth = thickness;
        lr.endWidth = thickness * 0.2f;

        // 👇 make tip thicker so we SEE the end
        var curve = new AnimationCurve();
        curve.AddKey(0f, thickness);          // start
        curve.AddKey(0.8f, thickness);        // most of the whip
        curve.AddKey(1f, thickness * 4f);     // fat tip
        lr.widthCurve = curve;

        points = new Vector3[segments];

        // find pull system on Marion (parent)
        pullSystem = GetComponentInParent<ThreadPullToPlayer>();

        StartCoroutine(DestroyAfter(lifetime));
    }

    void Update()
    {
        Vector3 start = transform.position;

        // ray from screen center
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 end = ray.GetPoint(length);

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 basePos = Vector3.Lerp(start, end, t);

            if (i < segments - 1)
            {
                float wave = Mathf.Sin(Time.time * waveSpeed - i * 0.5f)
                             * waveSize * (1f - t);

                Vector3 dir = (end - start).normalized;
                Vector3 side = Vector3.Cross(dir, Vector3.up).normalized;
                basePos += side * wave;
            }

            points[i] = basePos;
        }

        lr.SetPositions(points);

        // tip fx
        Vector3 tip = points[segments - 1];
        if (flashPrefab != null)
        {
            if (!spawnedFlash)
            {
                flashInstance = Instantiate(flashPrefab, tip, Quaternion.identity);
                spawnedFlash = true;
                flashDefaultScale = flashInstance.transform.localScale;
            }
            else
            {
                flashInstance.transform.position = tip;
            }
        }

        // hits (damage + pull)
        for (int i = 0; i < segments; i++)
        {
            Collider[] hits = Physics.OverlapSphere(points[i], pullCheckRadius);
            foreach (Collider col in hits)
            {
                if (col == null) continue;
                if (col.CompareTag("Player")) continue;

                // --- PULL: only once per whip instance ---
                if (enablePull && !pullTriggered && pullSystem != null && col.CompareTag(pullableTag))
                {
                    Rigidbody rb = col.attachedRigidbody;
                    if (rb != null)
                    {
                        pullSystem.Hook(rb);
                        pullTriggered = true;

                        if (flashInstance != null)
                            flashInstance.transform.localScale = flashDefaultScale * flashHitScale;

                        if (CameraShake.Instance != null)
                            CameraShake.Instance.Shake();
                    }
                }

                // --- DAMAGE ---
                Health target = col.GetComponent<Health>();
                if (target)
                {
                    if (!lastHit.ContainsKey(target) || Time.time - lastHit[target] > hitCooldown)
                    {
                        target.TakeDamage(damage);
                        lastHit[target] = Time.time;

                        if (flashInstance != null)
                            flashInstance.transform.localScale = flashDefaultScale * flashHitScale;

                        if (CameraShake.Instance != null)
                            CameraShake.Instance.Shake();
                    }
                }
            }
        }
    }

    IEnumerator DestroyAfter(float t)
    {
        yield return new WaitForSeconds(t);
        if (flashInstance != null) Destroy(flashInstance);
        Destroy(gameObject);
    }
}