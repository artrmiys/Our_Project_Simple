using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Health))]
public class FlyingBat : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;

    [Header("Idle fly")]
    public float idleRadius = 4f;
    public float idlePointChangeTime = 2.5f;
    public float minHoverHeight = 2f;
    public float maxHoverHeight = 15f;
    public float idleSpeed = 2.5f;
    public float bobbingAmplitude = 0.25f;
    public float bobbingSpeed = 4f;

    [Header("Attack")]
    public float attackRange = 6f;
    public float attackSpeed = 8f;
    public Vector2 attackCooldownRange = new Vector2(1.5f, 3.5f);
    public float retreatDistance = 5f;

    [Header("Damage / push")]
    public float attackDamage = 1f;
    public float attackForce = 6f;     // push on bite
    public float pushForce = 3f;       // push always
    public float pushCCForce = 2.5f;   // push for CharacterController
    public float hitCooldown = 0.5f;   // bite cd

    [Header("Bat HP")]
    public float batMaxHP = 3f;

    Health _health;
    Rigidbody _rb;

    Vector3 _spawnPos;
    Vector3 _idleTarget;
    float _idleTimer;
    bool _attacking;
    float _attackCooldownTimer;
    float _bobbingPhase;
    float _lastHitTime;

    void Awake()
    {
        _health = GetComponent<Health>();
        _health.onDied.AddListener(HandleDeath);
        _health.maxHP = batMaxHP;
        _health.currentHP = batMaxHP;

        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.useGravity = false;
            _rb.isKinematic = true;
        }

        if (!player)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) player = p.transform;
        }

        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Start()
    {
        _spawnPos = transform.position;
        PickNewIdlePoint();
        ResetAttackCooldown();
    }

    void Update()
    {
        Vector3 bob = GetBob();

        if (!player)
        {
            IdleMove(bob);
            return;
        }

        _attackCooldownTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);

        if (_attacking)
        {
            AttackMove(bob);
            return;
        }

        if (dist <= attackRange && _attackCooldownTimer <= 0f)
        {
            _attacking = true;
            return;
        }

        IdleMove(bob);
    }

    // ===== movement =====
    Vector3 GetBob()
    {
        _bobbingPhase += Time.deltaTime * bobbingSpeed;
        return Vector3.up * Mathf.Sin(_bobbingPhase) * bobbingAmplitude;
    }

    void IdleMove(Vector3 bob)
    {
        _idleTimer -= Time.deltaTime;
        if (_idleTimer <= 0f || Vector3.Distance(transform.position, _idleTarget) < 0.4f)
            PickNewIdlePoint();

        Vector3 dir = (_idleTarget - transform.position).normalized;
        transform.position += dir * idleSpeed * Time.deltaTime;
        transform.position += bob;

        if (dir.sqrMagnitude > 0.01f)
            transform.forward = dir;
    }

    void PickNewIdlePoint()
    {
        Vector2 circle = Random.insideUnitCircle * idleRadius;
        float y = Random.Range(minHoverHeight, maxHoverHeight);
        _idleTarget = _spawnPos + new Vector3(circle.x, y, circle.y);
        _idleTimer = idlePointChangeTime;
    }

    void AttackMove(Vector3 bob)
    {
        if (!player)
        {
            _attacking = false;
            return;
        }

        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * attackSpeed * Time.deltaTime;
        transform.position += bob;
        transform.forward = dir;
    }

    void ResetAttackCooldown()
    {
        _attackCooldownTimer = Random.Range(attackCooldownRange.x, attackCooldownRange.y);
    }

    // ===== collision =====
    void OnTriggerEnter(Collider other)
    {
        HandleContact(other);
    }

    void OnTriggerStay(Collider other)
    {
        HandleContact(other);
    }

    void HandleContact(Collider other)
    {
        // 1. always push player
        PushTarget(other);

        // 2. damage only in attack phase
        if (!_attacking) return;

        if (Time.time - _lastHitTime < hitCooldown)
            return;

        Health ph = other.GetComponent<Health>();
        if (!ph) ph = other.GetComponentInParent<Health>();

        if (ph)
        {
            ph.TakeDamage(attackDamage);
            _lastHitTime = Time.time;
        }

        // extra attack push
        PushTarget(other, attackForce);

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake();

        // fly away
        StartCoroutine(RetreatRoutine());
        ResetAttackCooldown();
    }

    // pushes CC or RB
    void PushTarget(Collider other, float extraForce = 0f)
    {
        Vector3 dir = (other.transform.position - transform.position).normalized;

        // rb push
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            rb.AddForce(dir * (pushForce + extraForce), ForceMode.Impulse);
            return;
        }

        // character controller push
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null)
        {
            // move once this frame
            Vector3 push = dir * (pushCCForce + extraForce);
            cc.Move(push * Time.deltaTime * 20f); // *20 to make it noticeable
        }
    }

    System.Collections.IEnumerator RetreatRoutine()
    {
        _attacking = false;

        if (!player)
            yield break;

        Vector3 away = (transform.position - player.position).normalized;
        Vector3 side = Random.insideUnitSphere * 1.5f;
        side.y = 0f;

        Vector3 target = transform.position + away * retreatDistance + side;
        target.y = Mathf.Clamp(target.y, minHoverHeight, maxHoverHeight);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            transform.position = Vector3.Lerp(transform.position, target, t);
            yield return null;
        }

        PickNewIdlePoint();
    }

    // ===== death =====
    void HandleDeath()
    {
        Destroy(gameObject);
    }
}
