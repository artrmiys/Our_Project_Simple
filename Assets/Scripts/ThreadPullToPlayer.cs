using UnityEngine;

public class ThreadPullToPlayer : MonoBehaviour
{
    [Header("Setup")]
    public Transform pullPoint;
    public LineRenderer line;

    [Header("Pull Settings")]
    public float pullSpeed = 8f;          // units per second
    public float stopDistance = 1.2f;
    public float maxObjectMass = 30f;

    private Rigidbody targetRb;
    private bool isPulling;

    void Awake()
    {
        if (line)
        {
            line.enabled = false;
            line.useWorldSpace = true;
            line.positionCount = 2;
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.E))
            Release();

        if (isPulling)
            UpdateLine();
    }

    void FixedUpdate()
    {
        if (!isPulling || targetRb == null || pullPoint == null) return;

        Vector3 toPlayer = pullPoint.position - targetRb.worldCenterOfMass;
        float sqrDist = toPlayer.sqrMagnitude;

        if (sqrDist <= stopDistance * stopDistance)
        {
            targetRb.velocity = Vector3.zero;
            targetRb.angularVelocity = Vector3.zero;
            Release();
            return;
        }

        // MovePosition respects collisions better than forcing velocity
        Vector3 step = toPlayer.normalized * (pullSpeed * Time.fixedDeltaTime);
        targetRb.MovePosition(targetRb.position + step);
    }

    public void Hook(Rigidbody rb)
    {
        if (rb == null) return;
        if (rb.mass > maxObjectMass) return;

        targetRb = rb;
        isPulling = true;

        if (line)
        {
            line.enabled = true;
            UpdateLine();
        }
    }

    void UpdateLine()
    {
        if (!line || pullPoint == null || targetRb == null) return;

        line.SetPosition(0, pullPoint.position);
        line.SetPosition(1, targetRb.worldCenterOfMass);
    }

    public void Release()
    {
        isPulling = false;
        targetRb = null;
        if (line) line.enabled = false;
    }
}