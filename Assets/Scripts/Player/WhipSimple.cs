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

    [Header("Flash Settings")]
    public GameObject flashPrefab;     // Particle System

    private LineRenderer lr;
    private Vector3[] points;

    // before particle
    private bool spawnedFlash = false;
    private GameObject flashInstance;

    void Start()
    {
        lr = GetComponent<LineRenderer>();

        lr.positionCount = segments;
        lr.startWidth = thickness;
        lr.endWidth = thickness * 0.2f;
        //lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

        lr.startColor = lr.endColor = color;

        points = new Vector3[segments];

        // start 
        StartCoroutine(DestroyAfter(lifetime));
    }

    void Update()
    {
        // first step
        Vector3 start = transform.position;
        Vector3 dir = transform.forward;   // direction
        float step = length / (segments - 1);

        for (int i = 0; i < segments; i++)
        {
            Vector3 basePos = start + dir * (step * i);

  
            float wave = Mathf.Sin(Time.time * waveSpeed - i * 0.5f)
                         * waveSize * (1f - (i / (float)(segments - 1)));

            // wave
            Vector3 side = Vector3.Cross(dir, Vector3.up).normalized;
            basePos += side * wave;

            points[i] = basePos;
        }

        lr.SetPositions(points);

        // end point
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
    }

    IEnumerator DestroyAfter(float t)
    {
        yield return new WaitForSeconds(t);

        if (flashInstance != null)
        {
            Destroy(flashInstance);
        }

        Destroy(gameObject);
    }
}
