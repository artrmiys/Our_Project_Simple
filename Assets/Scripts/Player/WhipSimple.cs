using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WhipSimple : MonoBehaviour
{
    [Header("Whip Settings")]
    public float length = 5f;
    public int segments = 12;
    public float lifetime = 0.4f;
    public float waveSize = 0.5f;
    public float waveSpeed = 20f;
    public float thickness = 0.05f;
    public Color color = Color.cyan;
    public int damage = 25;

    [Header("Flash Settings")]
    public GameObject flashPrefab;

    private LineRenderer lr;
    private Vector3[] points;

    private bool spawnedFlash = false;
    private GameObject flashInstance;

    private Vector3 whipDir;

    void Start()
    {
        lr = GetComponent<LineRenderer>();

        lr.positionCount = segments;
        lr.startWidth = thickness;
        lr.endWidth = thickness * 0.2f;
        lr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = color;

        points = new Vector3[segments];

        // dir cam
        whipDir = (Camera.main.transform.forward + Camera.main.transform.up * 0.4f).normalized;

        StartCoroutine(DestroyAfter(lifetime));
    }

    void Update()
    {
        Vector3 start = transform.position;
        float step = length / (segments - 1);

        // build line
        for (int i = 0; i < segments; i++)
        {
            Vector3 basePos = start + whipDir * (step * i);

            float wave = Mathf.Sin(Time.time * waveSpeed - i * 0.5f)
                         * waveSize * (1f - (i / (float)(segments - 1)));

            Vector3 side = Vector3.Cross(whipDir, Vector3.up).normalized;
            basePos += side * wave;

            points[i] = basePos;
        }

        lr.SetPositions(points);

        // fx at tip
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

        // hit check for all segments
        for (int i = 0; i < segments; i++)
        {
            Collider[] hits = Physics.OverlapSphere(points[i], 0.1f); // radius adjust
            foreach (Collider col in hits)
            {
                EnemyHealth enemy = col.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);

                    // shake cam
                    if (CameraShake.Instance != null)
                    {
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
