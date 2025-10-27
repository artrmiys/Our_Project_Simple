using UnityEngine;

public class TarakanAI : MonoBehaviour
{
    [Header("Movement")]
    public float minSpeed = 2f;
    public float maxSpeed = 6f;
    public float jumpChance = 0.01f;
    public float jumpForce = 3f;
    public float minWalkTime = 1f;
    public float maxWalkTime = 3f;
    public float minWaitTime = 0.5f;
    public float maxWaitTime = 2f;
    public float lifeTime = 10f;

    [Header("Combat")]
    public float damageDistance = 1.2f;  // 🔥 дистанция для атаки
    public float touchDamage = 1f;       // урон игроку
    public GameObject deathVFX;          // эффект смерти

    private float moveSpeed;
    private float walkTime;
    private float walkCounter;
    private float waitTime;
    private float waitCounter;
    private bool isWalking;
    private Rigidbody rb;
    private Vector3 moveDirection;
    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // random size
        float randomScale = Random.Range(0.4f, 1.0f);
        transform.localScale = new Vector3(randomScale * 0.4f, randomScale * 0.2f, randomScale);

        // random speed
        moveSpeed = Random.Range(minSpeed, maxSpeed);

        walkTime = Random.Range(minWalkTime, maxWalkTime);
        waitTime = Random.Range(minWaitTime, maxWaitTime);

        waitCounter = waitTime;
        walkCounter = walkTime;

        ChooseDirection();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (isWalking)
        {
            walkCounter -= Time.deltaTime;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            if (moveDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(moveDirection);

            if (Random.value < jumpChance * Time.deltaTime && IsGrounded())
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            if (walkCounter <= 0)
            {
                isWalking = false;
                waitCounter = waitTime;
            }
        }
        else
        {
            waitCounter -= Time.deltaTime;
            if (waitCounter <= 0)
                ChooseDirection();
        }

        // 🔥 Проверяем дистанцию до игрока
        if (player)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < damageDistance)
            {
                DamagePlayer();
            }
        }
    }

    void DamagePlayer()
    {
        // урон игроку
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(touchDamage);
            Debug.Log($"🪳 Tarakan damaged player for {touchDamage}");
        }

        // камера
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake();

        // эффект
        if (deathVFX != null)
            Instantiate(deathVFX, transform.position, Quaternion.identity);

        // смерть
        Destroy(gameObject);
    }

    public void ChooseDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        isWalking = true;
        walkCounter = walkTime;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.2f);
    }
}
