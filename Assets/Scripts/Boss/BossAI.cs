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

    [Header("Attack Timing (EnemyAI style)")]
    public float meleeWindup = 1f;          // момент удара
    public float skullWindup = 1f;          // момент выстрела
    public float meleeRecover = 0.55f;         // восстановление после удара
    public float skullRecover = 0.65f;         // восстановление после выстрела
    public float attackFailSafeTime = 2.0f;    // если застрял — выйти из атаки



    [Header("After Skull -> force approach")]
    public float approachAfterSkullTime = 1.2f;
    float approachUntil;

    [Header("Melee Damage")]
    public float meleeDamage = 8f;
    public float meleeExtraRange = 0.35f;

    [Header("Hit spam guard")]
    public float hitAnimCooldown = 0.25f;

    [Header("Camera Shake")]
    public bool shakeOnPlayerHit = true;

    [Header("Animator Params")]
    public string pSpeed = "Speed";
    public string pAttack = "Attack";
    public string pAttackID = "AttackID";
    public string pHit = "Hit";
    public string pIsDead = "isDead";
    public string pStunned = "Stunned";

    NavMeshAgent agent;
    Vector3 spawnPos;

    float nextAttackTime;
    float nextHitAnimTime;

    bool isDead;
    bool isStunned;

    bool isAttacking;
    float lastAttackStartTime;
    Coroutine attackRoutine;

    int nextMeleeId = 1;

    // to support animation events (avoid double damage/shot)
    int currentAttackId;
    bool actionDone;

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

        // failsafe: если застрял в атаке
        if (isAttacking && (Time.time - lastAttackStartTime) > attackFailSafeTime)
            FinishAttack();

        // while attacking: stay stopped, face player
        if (isAttacking)
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
            SetSpeedAnim(agent.desiredVelocity.magnitude);
            return;
        }

        FaceTarget(player.position);

        // force approach window after skull
        if (Time.time < approachUntil)
        {
            ChasePlayer();
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            // ranged (id=3)
            if (dist <= skullRange && dist > meleeRange + 0.3f)
            {
                StartAttack(3);
                return;
            }

            // melee (id=1/2)
            if (dist <= meleeRange)
            {
                int id = nextMeleeId;
                nextMeleeId = (nextMeleeId == 1) ? 2 : 1;
                StartAttack(id);
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
        SetSpeedAnim(agent.desiredVelocity.magnitude);
    }

    void StartAttack(int id)
    {
        if (isAttacking || isDead || isStunned) return;

        isAttacking = true;
        lastAttackStartTime = Time.time;

        currentAttackId = id;
        actionDone = false;

        StopAgent();
        SetSpeedAnim(0f);

        if (animator)
        {
            animator.SetInteger(pAttackID, id);
            animator.ResetTrigger(pAttack);
            animator.SetTrigger(pAttack);
        }

        nextAttackTime = Time.time + attackCooldown;

        if (id == 3)
        {
            // после выстрела — обязательно подойти
            approachUntil = Time.time + approachAfterSkullTime;
            if (skullMinion) skullMinion.PrepareShot();
        }

        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(AttackRoutine(id));
    }

    IEnumerator AttackRoutine(int id)
    {
        float windup = (id == 3) ? skullWindup : meleeWindup;
        float recover = (id == 3) ? skullRecover : meleeRecover;

        // windup
        yield return new WaitForSeconds(windup);

        if (isDead || isStunned) yield break;

        // действие: либо AnimationEvent, либо fallback по таймеру
        if (!actionDone)
        {
            actionDone = true;
            if (id == 3) DoSkullShot();
            else DoMeleeHit();
        }

        // recovery
        yield return new WaitForSeconds(recover);

        FinishAttack();
    }

    void DoMeleeHit()
    {
        if (!player) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > meleeRange + meleeExtraRange) return;

        Health h = player.GetComponent<Health>();
        if (!h) h = player.GetComponentInChildren<Health>();
        if (!h) h = player.GetComponentInParent<Health>();
        if (!h) return;

        h.TakeDamage(meleeDamage);

        if (shakeOnPlayerHit && CameraShake.Instance != null)
            CameraShake.Instance.Shake();
    }

    void DoSkullShot()
    {
        if (skullMinion) skullMinion.ShootAtPlayer();
    }

    void FinishAttack()
    {
        isAttacking = false;
        attackRoutine = null;

        if (isDead || isStunned || !player) return;

        agent.isStopped = false;
        agent.ResetPath();
        MoveTo(player.position);
    }

    // --- OPTIONAL: Animation Events (точная синхронизация) ---
    public void AnimEvent_MeleeHit()
    {
        if (!isAttacking || isDead || isStunned) return;
        if (currentAttackId == 3) return;
        if (actionDone) return;

        actionDone = true;
        DoMeleeHit();
    }

    public void AnimEvent_SkullShoot()
    {
        if (!isAttacking || isDead || isStunned) return;
        if (currentAttackId != 3) return;
        if (actionDone) return;

        actionDone = true;
        DoSkullShot();
    }

    // --------------------------------------------------------

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

    public void SetDead(bool dead)
    {
        isDead = dead;

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        isAttacking = false;
        actionDone = true;

        approachUntil = Mathf.Infinity;

        StopAgent();
        SetSpeedAnim(0f);

        if (animator)
        {
            animator.ResetTrigger(pAttack);
            animator.ResetTrigger(pHit);
            animator.SetInteger(pAttackID, 0);
            animator.SetBool(pStunned, false);
            animator.SetBool(pIsDead, dead);
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

        // не перебивать атаку хитом
        if (isAttacking) return;

        if (Time.time < nextHitAnimTime) return;
        nextHitAnimTime = Time.time + hitAnimCooldown;

        animator.SetTrigger(pHit);
    }

    public void OnDeath()
    {
        SetDead(true);
        enabled = false;

        if (skullMinion)
            skullMinion.gameObject.SetActive(false);
    }
}
