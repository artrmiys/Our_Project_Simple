using UnityEngine;

public class HealthBar3D : MonoBehaviour
{
    public Health playerHealth;       // ссылка на здоровье игрока
    public float rotationSpeed = 30f; // скорость вращения (град/сек)

    private Vector3 startScale;
    private Vector3 startPos;
    private float fullWidth;

    void Start()
    {
        startScale = transform.localScale;
        startPos = transform.position;
        fullWidth = startScale.x;

        if (!playerHealth)
            playerHealth = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Health>();
    }

    void Update()
    {
        if (!playerHealth) return;

        // === Масштаб по HP ===
        float hpPercent = Mathf.Clamp01(playerHealth.currentHP / playerHealth.maxHP);
        transform.localScale = new Vector3(fullWidth * hpPercent, startScale.y, startScale.z);

        // === Левый край остаётся на месте ===
        float offset = (fullWidth - fullWidth * hpPercent) * 0.5f;
        transform.position = startPos - transform.right * offset;

        // === Вращение по горизонтали (вокруг оси Y) ===
        transform.Rotate(Vector3.left * rotationSpeed * Time.deltaTime, Space.World);
    }
}
