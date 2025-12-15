using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BossAI : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public BossSkullMinion skullMinion;
    public Animator animator;

    [Header("Movement")]
    public float aggroRange = 30f;
    public float leashRange = 55f;
    public float stopDistance = 1.8f;
    public float rotateSpeed = 10f;

    [Header("Ranges")]
    public float meleeRange = 2.2f;
    public float skullRange = 10f;

    [Header("Cooldowns")]
    public float attackCooldown = 1.6f;

    [Header("Attack Lock (prevents running during attack)")]
    public float meleeAttackLock = 0.9f;
    public float skullAttackLock = 1.1f;
    float attackLockUntil;

    [Header("After Skull -> force approach (fixes 'attacks instead of chasing')")]
    public float approachAfterSkullTime = 1.2f;
    float approachUntil;

    [Header("Melee Damage")]
    public float meleeDamage = 8f;
    public float meleeHitDelay = 0.25f;
    public float meleeExtraRange = 0.35f;

    [Header("Skull Shot")]
    public float skullShotDelay = 0.35f;

    [Header("Hit spam guard")]
    public float hitAnimCooldown = 0.25f;

    [Header("Camera Shake (same as EnemyAI)")]
    public bool shakeOnPlayerHit = true;

    [Header("Animator Params")]
    public string pSpeed = "Speed";
    public string pAttack = "Attack";
    public string pAttackID = "AttackID";
    public string pHit = "Hit";
    public string pIsDead = "isDead";   // <-- было Dead
    public string pStunned = "Stunned";

    NavMeshAgent agent;
    Vector3 spawnPos;

    float nextAttackTime;
    float nextHitAnimTime;
    bool isDead;
    bool isStunned;

    int nextMeleeId = 1;

    Health myHealth;
    float lastHp = -1f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        spawnPos = transform.position;

        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        if (!skullMinion)
            skullMinion = GetComponentInChildren<BossSkullMinion>(true);

        if (skullMinion)
            skullMinion.player = player;

        myHealth = GetComponent<Health>();
    }

    void Start()
    {
        if (myHealth != null)
        {
            lastHp = myHealth.currentHP;
            myHealth.onDied.AddListener(OnDeath);
            myHealth.onHealthChanged.AddListener(OnHealthChanged);
        }
    }

    void OnHealthChanged(float cur, float max)
    {
        if (isDead) return;

        if (lastHp >= 0f && cur < lastHp - 0.001f)
            OnHit();

        lastHp = cur;
    }

    void Update()
    {
        if (!player || !agent || !agent.isOnNavMesh) return;

        if (isDead || isStunned)
        {
            StopAgent();
            SetSpeedAnim(0f);
            return;
        }

        if (Time.time < attackLockUntil)
        {
            StopAgent();
            SetSpeedAnim(0f);
            FaceTarget(player.position);
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > aggroRange)
        {
            StopAgent();
            SetSpeedAnim(0f);
            return;
        }

        if (dist > leashRange)
        {
            agent.isStopped = false;
            agent.stoppingDistance = 0.1f;
            MoveTo(spawnPos);
            SetSpeedAnim(agent.velocity.magnitude);
            return;
        }

        FaceTarget(player.position);

        if (Time.time < approachUntil)
        {
            ChasePlayer();
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            if (dist <= skullRange && dist > meleeRange + 0.3f)
            {
                DoAttack(3);
                return;
            }

            if (dist <= meleeRange)
            {
                int id = nextMeleeId;
                nextMeleeId = (nextMeleeId == 1) ? 2 : 1;
                DoAttack(id);
                return;
            }
        }

        ChasePlayer();
    }

    void ChasePlayer()
    {
        agent.isStopped = false;
        agent.stoppingDistance = stopDistance;
        MoveTo(player.position);
        SetSpeedAnim(agent.velocity.magnitude);
    }

    void DoAttack(int id)
    {
        StopAgent();
        SetSpeedAnim(0f);

        attackLockUntil = Time.time + (id == 3 ? skullAttackLock : meleeAttackLock);

        if (animator)
        {
            animator.SetInteger(pAttackID, id);
            animator.SetTrigger(pAttack);
        }

        nextAttackTime = Time.time + attackCooldown;

        if (id == 3 && skullMinion)
        {
            approachUntil = Time.time + approachAfterSkullTime;

            skullMinion.PrepareShot();
            StartCoroutine(SkullShotFallback());
        }
        else if (id == 1 || id == 2)
        {
            StartCoroutine(MeleeHitFallback());
        }
    }

    IEnumerator SkullShotFallback()
    {
        yield return new WaitForSeconds(skullShotDelay);
        if (isDead || isStunned) yield break;
        if (skullMinion) skullMinion.ShootAtPlayer();
    }

    IEnumerator MeleeHitFallback()
    {
        yield return new WaitForSeconds(meleeHitDelay);
        if (isDead || isStunned) yield break;
        if (!player) yield break;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > meleeRange + meleeExtraRange) yield break;

        Health h = player.GetComponent<Health>();
        if (!h) h = player.GetComponentInChildren<Health>();
        if (!h) h = player.GetComponentInParent<Health>();
        if (!h) yield break;

        h.TakeDamage(meleeDamage);

        if (shakeOnPlayerHit && CameraShake.Instance != null)
            CameraShake.Instance.Shake();
    }

    void MoveTo(Vector3 pos)
    {
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void FaceTarget(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * rotateSpeed);
    }

    void StopAgent()
    {
        agent.ResetPath();
        agent.isStopped = true;
    }

    void SetSpeedAnim(float agentSpeed)
    {
        if (!animator) return;

        float v = (agent.speed <= 0.01f) ? 0f : Mathf.Clamp01(agentSpeed / agent.speed);
        animator.SetFloat(pSpeed, v);
    }

    public void AnimEvent_SkullShoot()
    {
        if (skullMinion) skullMinion.ShootAtPlayer();
    }

    public void SetDead(bool dead)
    {
        isDead = dead;
        attackLockUntil = Mathf.Infinity;
        approachUntil = Mathf.Infinity;

        StopAgent();
        SetSpeedAnim(0f);

        if (animator)
        {
            // чтобы смерть не перебивалась триггерами
            animator.ResetTrigger(pAttack);
            animator.ResetTrigger(pHit);
            animator.SetInteger(pAttackID, 0);
            animator.SetBool(pStunned, false);

            animator.SetBool(pIsDead, dead);   // <-- isDead
        }
    }

    public void Stun(float sec)
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(StunCo(sec));
    }

    IEnumerator StunCo(float sec)
    {
        isStunned = true;
        if (animator) animator.SetBool(pStunned, true);
        yield return new WaitForSeconds(sec);
        isStunned = false;
        if (animator) animator.SetBool(pStunned, false);
    }

    public void OnHit()
    {
        if (!animator) return;
        if (isDead) return;

        if (Time.time < attackLockUntil) return;

        if (Time.time < nextHitAnimTime) return;
        nextHitAnimTime = Time.time + hitAnimCooldown;

        animator.SetTrigger(pHit);
    }

    public void OnDeath()
    {
        SetDead(true);
        StopAgent();
        enabled = false;

        if (skullMinion)
            skullMinion.gameObject.SetActive(false);
    }
}
