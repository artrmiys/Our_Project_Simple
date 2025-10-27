using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Animator animator;        // enemy animator
    public Transform player;         // player target

    [Header("Movement")]
    public Transform[] patrolPoints; // patrol route
    public float patrolSpeed = 1f;   // walk speed
    public float chaseSpeed = 5f;    // run speed
    public float visionRange = 8f;   // see distance
    public float visionAngle = 120f; // view angle

    [Header("Combat")]
    public float attackDistance = 1.8f;  // attack range
    public float attackForce = 5f;       // push power
    public float attackCooldown = 1.8f;  // delay attack
    public float attackDamage = 1f; // attack power


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

        // agent setup
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.angularSpeed = 720f;
        agent.autoBraking = false;

        // accel setup
        normalAcceleration = 20f;
        combatAcceleration = normalAcceleration * 2f;
        agent.acceleration = normalAcceleration;

        // death link
        health.onDied.AddListener(HandleDeath);
    }

    void Start()
    {
        StartPatrol();
    }

    void Update()
    {
        if (isDead || !player) return;
        if (isAttacking) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool canSee = CanSeePlayer();

        // near attack
        if (dist <= attackDistance && Time.time - lastAttackTime > attackCooldown)
        {
            Attack();
            return;
        }

        // see player
        if (canSee && dist <= visionRange)
        {
            lostPlayerTimer = 0f;
            if (!inCombat)
                EnterCombat();
            FollowPlayer();
        }
        // lost player
        else if (inCombat)
        {
            lostPlayerTimer += Time.deltaTime;
            if (lostPlayerTimer > 3f)
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

    // === states ===
    void EnterCombat()
    {
        inCombat = true;
        isChasing = true;
        agent.speed = chaseSpeed;
        agent.acceleration = combatAcceleration;
        agent.stoppingDistance = attackDistance;
        animator.SetBool("isChasing", true);
    }

    void ExitCombat()
    {
        inCombat = false;
        isChasing = false;
        agent.acceleration = normalAcceleration;
        animator.SetBool("isChasing", false);
    }

    // === patrol ===
    void Patrol()
    {
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.1f;

        if (!agent.hasPath || agent.remainingDistance < 0.2f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void StartPatrol()
    {
        ExitCombat();
        isAttacking = false;
        agent.isStopped = false;
        Patrol();
    }

    // === chase ===
    void FollowPlayer()
    {
        if (!player) return;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }

        if (Time.frameCount % 10 == 0)
        {
            agent.isStopped = false;
            bool success = agent.SetDestination(player.position);
            if (!success)
            {
                agent.ResetPath();
                agent.SetDestination(player.position);
            }
        }
    }

    // === vision ===
    bool CanSeePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > visionAngle * 0.5f) return false;
        if (Vector3.Distance(transform.position, player.position) > visionRange) return false;

        if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, visionRange))
            return hit.collider.CompareTag("Player");

        return false;
    }

    // === attack ===
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
      

        if (!player)
        {
            //Debug.Log("❌ player is null!");
            return;
        }

        Health playerHealth = player.GetComponent<Health>();
        if (!playerHealth)
        {
            //Debug.Log("❌ player has no Health component!");
            return;
        }

        playerHealth.TakeDamage(attackDamage);
        //Debug.Log($"✅ player took {attackDamage} damage");

        // отбрасывание
        Rigidbody prb = player.GetComponent<Rigidbody>();
        if (prb)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            prb.AddForce(dir * attackForce, ForceMode.Impulse);
        }

        // тряска камеры
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake();
    }


    void EndAttack()
    {
        isAttacking = false;
        agent.isStopped = false;

        if (player && !isDead)
        {
            isChasing = true;
            agent.ResetPath();
            agent.SetDestination(player.position);
        }
    }

    // === anim ===
    void UpdateAnimation()
    {
        if (!animator) return;
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    // === death ===
    void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        agent.isStopped = true;
        animator.SetBool("isDead", true);
        Destroy(gameObject, 3f);
    }
}
