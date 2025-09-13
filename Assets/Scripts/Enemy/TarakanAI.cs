using UnityEngine;

public class TarakanAI : MonoBehaviour
{
    public float minSpeed = 2f;
    public float maxSpeed = 6f;
    public float jumpChance = 0.01f;   // шанс прыжка
    public float jumpForce = 3f;
    public float minWalkTime = 1f;
    public float maxWalkTime = 3f;
    public float minWaitTime = 0.5f;
    public float maxWaitTime = 2f;
    public float lifeTime = 10f;       // сколько живёт таракан

    private float moveSpeed;
    private float walkTime;
    private float walkCounter;
    private float waitTime;
    private float waitCounter;
    private bool isWalking;
    private Rigidbody rb;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // случайный размер (сплющенный таракан)
        float randomScale = Random.Range(0.4f, 1.0f);
        transform.localScale = new Vector3(randomScale * 0.4f, randomScale * 0.2f, randomScale);

        // случайная скорость
        moveSpeed = Random.Range(minSpeed, maxSpeed);

        walkTime = Random.Range(minWalkTime, maxWalkTime);
        waitTime = Random.Range(minWaitTime, maxWaitTime);

        waitCounter = waitTime;
        walkCounter = walkTime;

        ChooseDirection();

        // не падаем набок
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // удаляем таракана через lifeTime секунд
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (isWalking)
        {
            walkCounter -= Time.deltaTime;

            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // поворачиваем длинной частью вперёд
            if (moveDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(moveDirection);

            // шанс подпрыгнуть
            if (Random.value < jumpChance * Time.deltaTime && IsGrounded())
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }

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
            {
                ChooseDirection();
            }
        }
    }

    public void ChooseDirection()
    {
        // случайное направление (360°)
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
