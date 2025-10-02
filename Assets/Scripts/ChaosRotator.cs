using UnityEngine;

public class ChaosRotator : MonoBehaviour
{
    [Header("Player")]
    public Transform player;     // player ref

    [Header("Rotate objs")]
    public Transform[] targets;  // 5 objs

    [Header("Move params")]
    public float moveRadius = 1f;      // move rad
    public float moveSpeed = 1f;       // move spd
    public float rotationSpeed = 20f;  // rot spd

    private Vector3[] baseOffsets;     // base offs
    private Vector3[] randomOffsets;   // rnd offs

    void Start()
    {
        baseOffsets = new Vector3[targets.Length];
        randomOffsets = new Vector3[targets.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            // save start offset
            baseOffsets[i] = targets[i].position - player.position;

            // set rnd offset
            randomOffsets[i] = Random.insideUnitSphere * moveRadius;
        }
    }

    void Update()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null) continue;

            // self rotate
            targets[i].Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            targets[i].Rotate(Vector3.right, rotationSpeed * 0.5f * Time.deltaTime);

            // final pos
            Vector3 targetPos = player.position + baseOffsets[i] + randomOffsets[i];

            // smooth move
            targets[i].position = Vector3.MoveTowards(
                targets[i].position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            // new rnd offset
            if (Vector3.Distance(targets[i].position, targetPos) < 0.1f)
            {
                randomOffsets[i] = Random.insideUnitSphere * moveRadius;
            }
        }
    }
}

