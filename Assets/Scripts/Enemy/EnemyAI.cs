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
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float visionRange = 10f;
    public float visionAngle = 60f;

    [Header("Combat")]
    public float attackDistance = 1.5f;
    public float attackForce = 5f;
    public float attackCooldown = 2f;

    [Header("Death")]
    public GameObject deathVFX;
    public AudioClip deathSfx;

    NavMeshAgent agent;
    Health health;

    int patrolIndex = 0;
    bool isDead;
    float lastAttackTime;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.angularSpeed = 720f;
        agent.acceleration = 20f;

        health.onDied.AddListener(HandleDeath);
    }

    void Start()
    {
        if (patrolPoints.Length > 0)
        {
            agent.speed = patrolSpeed;
            agent.stoppingDistance = 0.2f;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void Update()
    {
        if (isDead) return;

        float dist = player ? Vector3.Distance(transform.position, player.position) : Mathf.Infinity;

        if (player && dist <= attackDistance && Time.time - lastAttackTime > attackCooldown)
        {
            Attack();
        }
        else if (player && dist <= visionRange && CanSeePlayer())
        {
            Chase();
        }
        else if (patrolPoints.Length > 0)
        {
            Patrol();
        }

        UpdateAnimation();
    }

    // === Patrol & Chase ===
    void Patrol()
    {
        agent.speed = patrolSpeed;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void Chase()
    {
        agent.speed = chaseSpeed;
        agent.stoppingDistance = attackDistance - 0.1f;
        agent.isStopped = false;

        if (player) agent.SetDestination(player.position);
    }

    bool CanSeePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        return angle < visionAngle * 0.5f;
    }

    // === Combat ===
    void Attack()
    {
        lastAttackTime = Time.time;
        agent.isStopped = true;

        // включаем анимацию атаки
        animator.SetTrigger("Attack");

        // наносим урон сразу (без Animation Event)
        if (player)
        {
            Rigidbody prb = player.GetComponent<Rigidbody>();
            if (prb != null)
            {
                Vector3 dir = (player.position - transform.position).normalized;
                prb.AddForce(dir * attackForce, ForceMode.Impulse);
            }

            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake();
        }

        // через маленькую паузу снова идём за игроком
        Invoke(nameof(ResumeChase), 0.5f);
    }

    void ResumeChase()
    {
        if (!isDead && player)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    // === Animation ===
    void UpdateAnimation()
    {
        if (!animator) return;
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
    }

    // === Death ===
    void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        agent.isStopped = true;
        animator.SetBool("isDead", true);

        if (deathSfx) AudioSource.PlayClipAtPoint(deathSfx, transform.position);
        if (deathVFX) Instantiate(deathVFX, transform.position, Quaternion.identity);

        Destroy(gameObject, 3f); // удалим через 3 сек (примерная длина анимации)
    }
}
