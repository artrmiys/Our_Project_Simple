using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    [Header("Patrol / Chase")]
    public Transform[] patrolPoints;
    public float visionRange = 10f;
    public float visionAngle = 60f;
    public float chaseSpeed = 5f;
    public float patrolSpeed = 2f;
    public Transform player;

    [Header("Combat")]
    public string playerAttackTag = "PlayerAttack"; // ??? ???????? ????? ??????
    public float defaultDamageFromPlayer = 1f;      // ?? ??????, ???? ?? ???????? ??? DamageDealer
    public GameObject deathVFX;
    public AudioClip deathSfx;
    public float destroyDelay = 0.2f;

    NavMeshAgent agent;
    Health health;
    int currentPoint = 0;
    enum State { Patrol, Chase }
    State state = State.Patrol;
    bool isDead;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();

        // ??????? ????????? + ?????????????? RB ??? ?????????? ?????????
        var col = GetComponent<Collider>(); if (col) col.isTrigger = false;
        if (!TryGetComponent<Rigidbody>(out var rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; rb.useGravity = false;
        }

        health.onDied.AddListener(HandleDeath);
    }

    void Start()
    {
        agent.speed = patrolSpeed;
        if (patrolPoints != null && patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[currentPoint].position);
    }

    void Update()
    {
        if (isDead) return;

        switch (state)
        {
            case State.Patrol:
                Patrol();
                LookForPlayer();
                break;
            case State.Chase:
                Chase();
                break;
        }
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f && patrolPoints.Length > 0)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPoint].position);
        }
    }

    void LookForPlayer()
    {
        if (!player) return;
        Vector3 toP = (player.position - transform.position).normalized;
        float ang = Vector3.Angle(transform.forward, toP);
        if (Vector3.Distance(transform.position, player.position) < visionRange && ang < visionAngle * 0.5f)
        {
            state = State.Chase;
            agent.speed = chaseSpeed;
        }
    }

    void Chase()
    {
        if (!player) return;
        agent.SetDestination(player.position);
        if (Vector3.Distance(transform.position, player.position) > visionRange * 1.5f)
        {
            state = State.Patrol;
            agent.speed = patrolSpeed;
            if (patrolPoints.Length > 0)
                agent.SetDestination(patrolPoints[currentPoint].position);
        }
    }

    void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        if (agent) agent.isStopped = true;
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;

        if (deathSfx) AudioSource.PlayClipAtPoint(deathSfx, transform.position);
        if (deathVFX) Instantiate(deathVFX, transform.position, Quaternion.identity);

        Destroy(gameObject, destroyDelay);
    }
}
