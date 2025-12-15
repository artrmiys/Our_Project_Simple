using UnityEngine;

public class BossDamageReceiver : MonoBehaviour
{
    public BossAI boss;

    void Awake()
    {
        if (!boss) boss = GetComponent<BossAI>();
    }

    // call this from your Health when damaged
    public void OnDamaged()
    {
        if (boss) boss.OnHit();
    }

    // call this from your Health when dead
    public void OnDied()
    {
        if (boss) boss.OnDeath();
    }
}
