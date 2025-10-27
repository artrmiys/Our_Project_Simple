using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Min(0.1f)] public float maxHP = 50f;
    [HideInInspector] public float currentHP;

    [Header("Popup")]
    public GameObject damagePopupPrefab; // prefab
    public Transform popupPoint;         // point above head
    private Color playerHitColor = Color.red;              // 💚 враг получает урон
    private Color enemyHitColor = Color.green;                // 💚 враг получает урон


    [Header("Events")]
    public UnityEvent<float, float> onHealthChanged; // (current, max)
    public UnityEvent onDied;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float amount)
    {
        if (currentHP <= 0f || amount <= 0f) return;

        currentHP = Mathf.Max(0f, currentHP - amount);

        // === POPUP DEBUG ===
        if (damagePopupPrefab && popupPoint)
        {
            Vector3 spawnPos = popupPoint.position + Vector3.up * 0.2f;
            //Debug.Log($"[Popup] Spawning popup at {spawnPos} for {gameObject.name} (damage={amount})");

            GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
            DamagePopup dp = popup.GetComponent<DamagePopup>();

            if (dp != null)
            {
                Color popupColor = CompareTag("Player") ? playerHitColor : enemyHitColor;

                dp.Setup(amount, popupColor);

                //Debug.Log($"[Popup] Popup Setup() called successfully for {gameObject.name}");
            }
            else
            {
                //Debug.LogWarning($"[Popup] DamagePopup script missing on prefab!");
            }
        }
        else
        {
            //Debug.LogWarning($"[Popup] Missing damagePopupPrefab or popupPoint on {gameObject.name}");
        }

        // === EVENTS ===
        onHealthChanged?.Invoke(currentHP, maxHP);

        // === DEATH ===
        if (currentHP <= 0f)
        {
            Debug.Log($"{gameObject.name} died");
            Die();
        }
    }


    public void Heal(float amount)
    {
        if (currentHP <= 0f || amount <= 0f) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        onHealthChanged?.Invoke(currentHP, maxHP);
    }

    void Die()
    {
        onDied?.Invoke();
        Debug.Log($"{gameObject.name} died");
        Destroy(gameObject);
    }
}
