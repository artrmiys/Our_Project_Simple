using System.Collections;
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
    public float thickness = 0.05f; // line wid
    public Color color = Color.cyan;// line col
    public int damage = 25;         // hit dmg

    [Header("Flash set")]
    public GameObject flashPrefab;  // flash fx

    private LineRenderer lr;        // line ref
    private Vector3[] points;       // line pts
    private bool spawnedFlash = false;
    private GameObject flashInstance;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = segments;
        lr.startWidth = thickness;
        lr.endWidth = thickness * 0.2f;
        lr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = color;

        points = new Vector3[segments];
        StartCoroutine(DestroyAfter(lifetime));
    }

    void Update()
    {
        // start pos
        Vector3 start = transform.position;

        // end pos
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 end = ray.GetPoint(length);

        // build line
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 basePos = Vector3.Lerp(start, end, t);

            if (i < segments - 1) // wave add
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

        // flash tip
        Vector3 tip = points[segments - 1];
        if (flashPrefab != null)
        {
            if (!spawnedFlash)
            {
                flashInstance = Instantiate(flashPrefab, tip, Quaternion.identity);
                spawnedFlash = true;
            }
            else
            {
                flashInstance.transform.position = tip;
            }
        }

        // hit check
        for (int i = 0; i < segments; i++)
        {
            Collider[] hits = Physics.OverlapSphere(points[i], 0.15f);
            foreach (Collider col in hits)
            {
                EnemyHealth enemy = col.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    if (CameraShake.Instance != null)
                        CameraShake.Instance.Shake();
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
