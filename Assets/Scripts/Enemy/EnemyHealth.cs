using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHP = 50;
    private int currentHP;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{gameObject.name} damage {damage}, left HP: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} died");
        // animation
        Destroy(gameObject);
    }
}
