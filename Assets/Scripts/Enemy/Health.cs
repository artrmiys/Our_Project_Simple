using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Min(0.1f)] public float maxHP = 3f;
    [HideInInspector] public float currentHP;

    [Header("Events")]
    public UnityEvent<float, float> onHealthChanged; // (current, max)
    public UnityEvent onDied;

    void Awake() { currentHP = maxHP; }

    public void TakeDamage(float amount)
    {
        if (currentHP <= 0f || amount <= 0f) return;
        currentHP = Mathf.Max(0f, currentHP - amount);
        onHealthChanged?.Invoke(currentHP, maxHP);
        if (currentHP <= 0f) onDied?.Invoke();
    }

    public void Heal(float amount)
    {
        if (currentHP <= 0f || amount <= 0f) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        onHealthChanged?.Invoke(currentHP, maxHP);
    }
}