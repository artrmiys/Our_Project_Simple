using UnityEngine;

public class Firefly : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public float changeDirTime = 3f;
    public Vector3 areaSize = new Vector3(40, 20, 40);

    private Vector3 moveDirection;
    private float timer;

    void Start()
    {
        timer = changeDirTime;
        ChooseNewDirection();
    }

    void Update()
    {
        // менять направление
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            ChooseNewDirection();
            timer = changeDirTime;
        }

        // движение
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // лёгкое плавание вверх-вниз
        float floatY = Mathf.Sin(Time.time * 2f + transform.GetInstanceID()) * 0.2f;
        transform.position = new Vector3(transform.position.x, transform.position.y + floatY * Time.deltaTime, transform.position.z);

        // ограничение по области
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -areaSize.x / 2f, areaSize.x / 2f);
        pos.y = Mathf.Clamp(pos.y, 0f, areaSize.y);
        pos.z = Mathf.Clamp(pos.z, -areaSize.z / 2f, areaSize.z / 2f);
        transform.position = pos;
    }

    void ChooseNewDirection()
    {
        float angleY = Random.Range(0f, 360f);
        float angleX = Random.Range(-10f, 10f);
        moveDirection = Quaternion.Euler(angleX, angleY, 0) * Vector3.forward;
    }
}
