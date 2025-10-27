using UnityEngine;

public class WhipPickup : MonoBehaviour
{
    [Header("sound")]
    public AudioSource audioSource;
    public AudioClip pickupSound;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // give whip
            PlayerAttack pa = other.GetComponent<PlayerAttack>();
            if (pa != null)
            {
                pa.CollectWhip();
            }

            // play sound
            if (audioSource && pickupSound)
                audioSource.PlayOneShot(pickupSound);

            // hide object
            gameObject.SetActive(false);
        }
    }
}

