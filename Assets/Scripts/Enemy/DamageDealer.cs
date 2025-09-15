using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageDealer : MonoBehaviour
{
    public float damage = 1f;
    public string damageTargetTag = "Enemy";   // кого бить (по тегу)
    public bool oneHitPerSwing = true;         // чтобы не тикало каждый кадр
    bool _hitThisSwing;

    void OnEnable() { _hitThisSwing = false; }  // каждый раз при включении хитбокса — новый свинг

    void OnTriggerEnter(Collider other)
    {
        if (oneHitPerSwing && _hitThisSwing) return;
        if (!other.CompareTag(damageTargetTag)) return;

        var hp = other.GetComponentInParent<Health>();
        if (hp == null) return;

        hp.TakeDamage(damage);
        _hitThisSwing = true;
    }
}