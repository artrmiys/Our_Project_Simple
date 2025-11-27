using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform player;

    [Header("Movement")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 1f;
    public float chaseSpeed = 5f;
    public float visionRange = 8f;
    public float visionAngle = 120f;

    [Header("Combat")]
    public float attackDistance = 1.8f;
    public float attackForce = 5f;
    public float attackCooldown = 1.8f;
    public float attackDamage = 1f;

    [Header("Chase settings")]
    public float loseSightTime = 5f;
    public float maxChaseDistance = 25f;

    NavMeshAgent agent;
    Health health;

    int patrolIndex = 0;
    bool isDead;
    bool isChasing;
    bool isAttacking;
    bool inCombat;
    float lastAttackTime;
    float lostPlayerTimer;
    float normalAcceleration;
    float combatAcceleration;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.angularSpeed = 720f;
        agent.autoBraking = false;

        normalAcceleration = 20f;
        combatAcceleration = normalAcceleration * 2f;
        agent.acceleration = normalAcceleration;

        health.onDied.AddListener(HandleDeath);

    }

    void Start()
    {
        // auto-find player if not set
        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // DEBUG ↓↓↓
        Debug.Log($"{name} START: player={player}, onNav={agent.isOnNavMesh}");

        // try snap to NavMesh
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position); // put on navmesh
                Debug.Log($"{name}: warped to NavMesh at {hit.position}");
            }
            else
            {
                Debug.LogError($"{name}: NO NAVMESH NEARBY!");
            }
        }

        StartPatrol();
    }


    void Update()
    {
        if (isDead || !player) return;
        if (isAttacking) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool canSee = CanSeePlayer();

        // атака
        if (dist <= attackDistance && Time.time - lastAttackTime > attackCooldown)
        {
            Attack();
            return;
        }

        // видим игрока
        if (canSee && dist <= visionRange)
        {
            if (!inCombat)
                EnterCombat();

            lostPlayerTimer = 0f;
            FollowPlayer();
        }
        else if (inCombat)
        {
            FollowPlayer();

            lostPlayerTimer += Time.deltaTime;
            if (lostPlayerTimer > loseSightTime && dist > visionRange * 1.5f && dist > attackDistance + 0.5f)
            {
                ExitCombat();
                StartPatrol();
            }
        }
        else
        {
            Patrol();
        }

        UpdateAnimation();
    }

    void EnterCombat()
    {
        inCombat = true;
        isChasing = true;
        agent.speed = chaseSpeed;
        agent.acceleration = combatAcceleration;
        agent.stoppingDistance = attackDistance;
        animator.SetBool("isChasing", true);
        lostPlayerTimer = 0f;
    }

    void ExitCombat()
    {
        inCombat = false;
        isChasing = false;
        agent.acceleration = normalAcceleration;
        agent.stoppingDistance = 0.1f;
        animator.SetBool("isChasing", false);
    }

    void Patrol()
    {
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.1f;

        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        if (!agent.hasPath || agent.remainingDistance < 0.2f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void StartPatrol()
    {
        isAttacking = false;
        agent.isStopped = false;
        Patrol();
    }

    void FollowPlayer()
    {
        if (!player) return;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }

        agent.isStopped = false;

        bool success = agent.SetDestination(player.position);
        if (!success)
        {
            agent.ResetPath();
            agent.SetDestination(player.position);
        }
    }

    bool CanSeePlayer()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < 2f)
            return true;

        Vector3 dir = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > visionAngle * 0.5f) return false;
        if (dist > visionRange) return false;

        if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, visionRange))
            return hit.collider.CompareTag("Player");

        return false;
    }

    void Attack()
    {
        if (isAttacking) return;

        lastAttackTime = Time.time;
        isAttacking = true;
        agent.isStopped = true;

        animator.SetTrigger("Attack");
        Invoke(nameof(PerformAttackHit), 0.35f);
        Invoke(nameof(EndAttack), 0.9f);
    }

    void PerformAttackHit()
    {
        if (!player) return;

        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth)
            playerHealth.TakeDamage(attackDamage);

        Rigidbody prb = player.GetComponent<Rigidbody>();
        if (prb)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            prb.AddForce(dir * attackForce, ForceMode.Impulse);
        }

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake();
    }

    void EndAttack()
    {
        isAttacking = false;
        agent.isStopped = false;

        if (player && !isDead)
        {
            if (!inCombat) EnterCombat();
            agent.ResetPath();
            agent.SetDestination(player.position);
        }
    }

    void UpdateAnimation()
    {
        if (!animator) return;
        if (isDead)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("isDead", true);
            return;
        }
        if (isAttacking) return;

        float speed = agent.velocity.magnitude;
        bool closeToPlayer = inCombat && agent.remainingDistance <= attackDistance + 0.2f;

        animator.SetFloat("Speed", speed);
        animator.SetBool("isChasing", inCombat && !closeToPlayer);
    }

    void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        agent.isStopped = true;
        animator.SetBool("isDead", true);
        Destroy(gameObject, 3f);
    }

}
