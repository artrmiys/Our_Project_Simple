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

    [Header("Movement Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 1f;
    public float chaseSpeed = 5f;

    [Header("Vision Settings")]
    public float visionRange = 10f;
    public float visionAngle = 150f;

    [Header("Combat Settings")]
    public float attackDistance = 1.8f;
    public float attackCooldown = 1.8f;
    public float attackDamage = 1f;

    [Header("Chase Behavior")]
    public float loseSightTime = 5f;

    // Internal
    private NavMeshAgent agent;
    private Health health;
    private int patrolIndex = 0;
    private bool isDead;
    private bool inCombat;
    private bool isAttacking;
    private float lastAttackTime;
    private float lostPlayerTimer;

    private float baseAcceleration;
    private float combatAcceleration;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        agent.updatePosition = true;
        agent.updateRotation = false;
        agent.angularSpeed = 720f;
        agent.autoBraking = false;

        baseAcceleration = 20f;
        combatAcceleration = baseAcceleration * 2f;
        agent.acceleration = baseAcceleration;

        health.onDied.AddListener(OnDeath);
    }

    void Start()
    {
        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }

        StartPatrol();
    }

    void Update()
    {
        if (isDead || !player) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool seesPlayer = CanSeePlayer();

        // ATTACK ANIMATION PHASE
        if (isAttacking)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            UpdateAnimation();
            RotateTowardsMovement();
            return;
        }

        // MELEE RANGE LOGIC
        if (dist <= attackDistance)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            if (!inCombat)
                EnterCombat();

            if (Time.time - lastAttackTime > attackCooldown)
                StartAttack();
        }
        else
        {
            // NORMAL MOVEMENT
            agent.isStopped = false;

            if (seesPlayer && dist <= visionRange)
            {
                if (!inCombat)
                    EnterCombat();

                lostPlayerTimer = 0f;
                ChasePlayer();
            }
            else if (inCombat)
            {
                ChasePlayer();
                lostPlayerTimer += Time.deltaTime;

                if (lostPlayerTimer > loseSightTime && dist > attackDistance + 0.5f)
                {
                    ExitCombat();
                    StartPatrol();
                }
            }
            else
            {
                Patrol();
            }
        }

        UpdateAnimation();
        RotateTowardsMovement();
    }

    // ROTATION
    void RotateTowardsMovement()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 dir = agent.velocity.normalized;
            dir.y = 0;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    // VISION
    bool CanSeePlayer()
    {
        if (!player) return false;

        // Eye positions (enemy and player)
        Vector3 eyePos = transform.position + Vector3.up * 1.6f;
        Vector3 targetPos = player.position + Vector3.up * 1.2f;

        Vector3 dir = targetPos - eyePos;
        float dist = dir.magnitude;

        // Distance check
        if (dist > visionRange)
            return false;

        dir.Normalize();

        // Vision cone check
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > visionAngle * 0.5f)
            return false;

        // Raycast that ignores the "Enemies" layer
        int layerMask = ~LayerMask.GetMask("Enemies");

        if (Physics.Raycast(eyePos, dir, out RaycastHit hit, visionRange, layerMask))
        {
            // Must hit the player
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    // PATROL
    void Patrol()
    {
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.1f;

        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (!agent.hasPath || agent.remainingDistance < 0.2f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void StartPatrol()
    {
        isAttacking = false;
        inCombat = false;
        agent.acceleration = baseAcceleration;
        agent.isStopped = false;
        Patrol();
    }

    // CHASE
    void ChasePlayer()
    {
        if (!player) return;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.acceleration = combatAcceleration;

        agent.SetDestination(player.position);
    }

    // COMBAT STATES
    void EnterCombat()
    {
        inCombat = true;
        agent.stoppingDistance = attackDistance;
        agent.acceleration = combatAcceleration;

        animator.SetBool("isChasing", true);
    }

    void ExitCombat()
    {
        inCombat = false;
        agent.stoppingDistance = 0.1f;
        agent.acceleration = baseAcceleration;

        animator.SetBool("isChasing", false);
    }

    // ATTACK
    void StartAttack()
    {
        if (isAttacking) return;

        lastAttackTime = Time.time;
        isAttacking = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        animator.SetTrigger("Attack");

        Invoke(nameof(ApplyAttackDamage), 0.35f);
        Invoke(nameof(FinishAttack), 0.9f);
    }

    void ApplyAttackDamage()
    {
        if (!player) return;

        Health ph = player.GetComponent<Health>();
        if (!ph) ph = player.GetComponentInChildren<Health>();
        if (!ph) ph = player.GetComponentInParent<Health>();

        if (ph)
        {
            ph.TakeDamage(attackDamage);
            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake();
        }
        else
        {
            Debug.LogWarning("Enemy tried to damage player but could NOT find Health!");
        }
    }

    void FinishAttack()
    {
        isAttacking = false;

        if (!isDead && player)
        {
            if (!inCombat)
                EnterCombat();

            agent.isStopped = false;
            agent.ResetPath();
            agent.SetDestination(player.position);
        }
    }

    // ANIMATION
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

        bool shouldMove =
            speed > 0.05f ||
            (agent.hasPath && agent.remainingDistance > agent.stoppingDistance + 0.1f);

        animator.SetFloat("Speed", shouldMove ? 1f : 0f);
        animator.SetBool("isChasing", inCombat && shouldMove);
    }

    // DEATH
    void OnDeath()
    {
        if (isDead) return;

        isDead = true;
        agent.isStopped = true;

        animator.SetBool("isDead", true);

        Destroy(gameObject, 3f);
    }
}