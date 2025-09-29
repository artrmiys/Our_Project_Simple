using UnityEngine;

public class JumpScareEnemy : MonoBehaviour
{
    [Header("settings")]
    public float triggerDistance = 5f;
    public float jumpDuration = 0.1f;   // время полёта
    public float jumpHeight = 2f;       // высота дуги
    public int damage = 20;

    [Header("refs")]
    public Transform player;

    private bool triggered = false;
    private bool attacking = false;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float attackTimer = 0f;

    void Update()
    {
        if (!triggered && player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= triggerDistance)
            {
                TriggerAttack();
            }
        }

        if (attacking)
        {
            attackTimer += Time.deltaTime;
            float t = Mathf.Clamp01(attackTimer / jumpDuration);

            // траектория с дугой
            Vector3 flat = Vector3.Lerp(startPos, targetPos, t);
            float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            transform.position = new Vector3(flat.x, flat.y + height, flat.z);

            if (t >= 1f)
            {
                attacking = false;
                // удар гарантирован в конце полёта
                HitPlayer();
                Destroy(gameObject, 0.5f);
            }
        }
    }

    void TriggerAttack()
    {
        triggered = true;
        attacking = true;
        attackTimer = 0f;
        startPos = transform.position;
        targetPos = player.position; // цель фиксируется при старте прыжка
    }

    void HitPlayer()
    {
        if (player != null)
        {
            Health hp = player.GetComponent<Health>();
            if (hp != null)
                hp.TakeDamage(damage);

            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake();
        }
    }
}
