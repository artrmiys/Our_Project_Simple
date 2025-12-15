using System.Collections;
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

    [Header("Attack Timing")]
    public float attackWindup = 0.35f;     // when damage happens
    public float attackRecover = 0.55f;    // recovery after hit
    public float attackFailSafeTime = 2.0f; // if stuck in attack -> force exit

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

    private Coroutine attackRoutine;
    [Header("Camera Shake (on hit player)")]
    public bool shakeOnPlayerHit = true;
    public float shakeDuration = 0.12f;
    public float shakeStrength = 0.15f;


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

        // failsafe: if attack got stuck, force exit
        if (isAttacking && (Time.time - lastAttackTime) > attackFailSafeTime)
            FinishAttack();

        // while attacking: stay stopped, face player
        if (isAttacking)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            RotateTowardsTarget(player.position);
            UpdateAnimation();
            return;
        }

        // melee range
        if (dist <= attackDistance)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            if (!inCombat) EnterCombat();

            // attack if cooldown ready
            if (Time.time - lastAttackTime >= attackCooldown)
                StartAttack();

            RotateTowardsTarget(player.position);
        }
        else
        {
            agent.isStopped = false;

            if (seesPlayer && dist <= visionRange)
            {
                if (!inCombat) EnterCombat();
                lostPlayerTimer = 0f;
                ChasePlayer();
            }
            else if (inCombat)
            {
                // keep chasing for a while even if lost
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

    // ROTATION (movement-based)
    void RotateTowardsMovement()
    {
        Vector3 v = agent.desiredVelocity; // faster response than agent.velocity
        if (v.sqrMagnitude > 0.05f)
        {
            v.y = 0f;
            Quaternion targetRot = Quaternion.LookRotation(v.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    // ROTATION (target-based)
    void RotateTowardsTarget(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 12f);
    }

    // VISION
    bool CanSeePlayer()
    {
        if (!player) return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.6f;
        Vector3 targetPos = player.position + Vector3.up * 1.2f;

        Vector3 dir = targetPos - eyePos;
        float dist = dir.magnitude;

        if (dist > visionRange) return false;

        dir.Normalize();

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > visionAngle * 0.5f) return false;

        int layerMask = ~LayerMask.GetMask("Enemies");

        if (Physics.Raycast(eyePos, dir, out RaycastHit hit, visionRange, layerMask))
            return hit.collider.CompareTag("Player");

        return false;
    }

    // PATROL
    void Patrol()
    {
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.15f;

        if (patrolPoints == null || patrolPoints.Length == 0) return;
        if (!agent.isOnNavMesh) return;
        if (agent.pathPending) return;

        // reached: используем stoppingDistance (а не 0.2) — это ключ
        bool reached = !agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.05f;

        // если путь сломан/недостижим — тоже переключаем точку
        if (reached || agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;

            Vector3 target = patrolPoints[patrolIndex].position;

            // прижимаем цель к ближайшей точке NavMesh
            if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                target = hit.position;

            agent.SetDestination(target);
        }
    }


    void StartPatrol()
    {
        isAttacking = false;
        inCombat = false;
        lostPlayerTimer = 0f;

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

        agent.speed = chaseSpeed;
        agent.acceleration = combatAcceleration;
        agent.stoppingDistance = attackDistance;

        agent.SetDestination(player.position);
    }

    // COMBAT STATES
    void EnterCombat()
    {
        inCombat = true;
        agent.stoppingDistance = attackDistance;
        agent.acceleration = combatAcceleration;
    }

    void ExitCombat()
    {
        inCombat = false;
        agent.stoppingDistance = 0.1f;
        agent.acceleration = baseAcceleration;
    }

    // ATTACK (Coroutine вместо Invoke)
    void StartAttack()
    {
        if (isAttacking || isDead) return;

        lastAttackTime = Time.time;
        isAttacking = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (animator)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }

        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        // windup
        yield return new WaitForSeconds(attackWindup);

        if (!isDead && player)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= attackDistance + 0.3f)
                ApplyAttackDamage();
        }

        // recovery
        yield return new WaitForSeconds(attackRecover);

        FinishAttack();
    }

    void ApplyAttackDamage()
    {
        if (isDead || !player) return;

        Health ph = player.GetComponent<Health>();
        if (!ph) ph = player.GetComponentInChildren<Health>();
        if (!ph) ph = player.GetComponentInParent<Health>();

        if (ph)
        {
            ph.TakeDamage(attackDamage);
            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake();
        }
    }

    void FinishAttack()
    {
        isAttacking = false;
        attackRoutine = null;

        if (isDead || !player) return;

        if (!inCombat) EnterCombat();

        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination(player.position);
    }

    // ANIMATION
    void UpdateAnimation()
    {
        if (!animator) return;

        if (isDead)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("isChasing", false);
            animator.SetBool("isDead", true);
            return;
        }

        if (isAttacking)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("isChasing", false);
            return;
        }

        // use desiredVelocity -> fixes late walk when chasing
        float speed = agent.desiredVelocity.magnitude;

        bool shouldMove =
            speed > 0.05f ||
            (agent.hasPath && agent.remainingDistance > agent.stoppingDistance + 0.1f);

        animator.SetFloat("Speed", shouldMove ? 1f : 0f);
        animator.SetBool("isChasing", inCombat && shouldMove);
    }

    // DEATH (Health destroys with delay; here only stop logic + anim flag)
    void OnDeath()
    {
        if (isDead) return;

        isDead = true;

        // stop attack routine safely
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        isAttacking = false;
        inCombat = false;

        if (agent)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }

        if (animator)
        {
            animator.ResetTrigger("Attack");
            animator.SetBool("isChasing", false);
            animator.SetFloat("Speed", 0f);
            animator.SetBool("isDead", true);
        }
    }
}
