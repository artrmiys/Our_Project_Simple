using UnityEngine;

public class WhipPickup : MonoBehaviour
{
    [Header("sound")]
    public AudioSource audioSource;   // источник звука (можно на этом объекте)
    public AudioClip pickupSound;     // звук подбора

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // даём кнут игроку
            PlayerAttack pa = other.GetComponent<PlayerAttack>();
            if (pa != null)
            {
                pa.CollectWhip();
            }

            // звук подбора
            if (audioSource && pickupSound)
                audioSource.PlayOneShot(pickupSound);

            // скрываем объект
            gameObject.SetActive(false);
        }
    }
}
