using UnityEngine;

public class JumpScareEnemy : MonoBehaviour
{
    [Header("settings")]
    public float triggerDistance = 5f;
    public float jumpDuration = 0.1f;
    public float jumpHeight = 2f;
    public int damage = 20;
    public float destroyDelay = 0.9f;

    [Header("refs")]
    public Transform player;
    public GameObject smokeEffectPrefab;

    private bool triggered = false;
    private bool attacking = false;

    private Vector3 startPos;
    private float attackTimer = 0f;

    void Update()
    {
        if (!triggered && player != null)
        {
            if (Vector3.Distance(transform.position, player.position) <= triggerDistance)
                TriggerAttack();
        }

        if (attacking)
        {
            attackTimer += Time.deltaTime;
            float t = Mathf.Clamp01(attackTimer / jumpDuration);

            // обновляем targetPos каждый кадр
            Vector3 targetPos = new Vector3(player.position.x, startPos.y, player.position.z);

            Vector3 flat = Vector3.Lerp(startPos, targetPos, t);
            float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;

            transform.position = new Vector3(flat.x, flat.y + height, flat.z);

            if (t >= 1f)
            {
                attacking = false;
                HitPlayer();

                if (smokeEffectPrefab != null)
                    Instantiate(smokeEffectPrefab, transform.position, Quaternion.identity);

                Destroy(gameObject, destroyDelay);
            }
        }
    }

    void TriggerAttack()
    {
        triggered = true;
        attacking = true;
        attackTimer = 0f;

        startPos = transform.position;
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