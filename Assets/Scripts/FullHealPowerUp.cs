using UnityEngine;

public class FullHealPowerUp : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player";

    [Header("Feedback (optional)")]
    public AudioSource audioSource;
    public AudioClip pickupClip;
    [Range(0f, 1f)] public float volume = 1f;
    public ParticleSystem pickupVfx;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        var health = other.GetComponentInParent<Health>();
        if (health == null) return;

        health.RestoreToFull();

        if (pickupVfx != null)
        {
            pickupVfx.transform.parent = null;
            pickupVfx.Play();
            Destroy(pickupVfx.gameObject, pickupVfx.main.duration + pickupVfx.main.startLifetime.constantMax);
        }

        if (audioSource != null && pickupClip != null)
            audioSource.PlayOneShot(pickupClip, volume);

        Destroy(gameObject);
    }
}