using UnityEngine;
using UnityEngine.AI;

public class NavMeshDebug : MonoBehaviour
{
    public Transform target;
    NavMeshAgent a;

    void Awake() => a = GetComponent<NavMeshAgent>();

    void Update()
    {
        if (!a || !target) return;

        // пробуем найти валидную точку на NavMesh около игрока
        NavMeshHit hit;
        bool ok = NavMesh.SamplePosition(target.position, out hit, 3f, NavMesh.AllAreas);

        bool set = ok && a.SetDestination(hit.position);

        Debug.Log(
            $"[BossNav] onNav={a.isOnNavMesh} enabled={a.enabled} stopped={a.isStopped} " +
            $"vel={a.velocity.magnitude:F2} hasPath={a.hasPath} path={a.pathStatus} " +
            $"sampleOk={ok} setDest={set} dist={a.remainingDistance:F2} " +
            $"agentPos={transform.position} dest={(ok ? hit.position.ToString() : "none")}"
        );
    }
}
