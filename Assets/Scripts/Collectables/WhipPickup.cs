using UnityEngine;

public class WhipPickup : MonoBehaviour
{
    [Header("pickup sound")]
    public AudioClip pickupSound;     // звук подбора

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // включаем кнут у игрока
            PlayerAttack pa = other.GetComponent<PlayerAttack>();
            if (pa != null)
                pa.CollectWhip();

            // проигрываем звук подбора в точке предмета
            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            // убираем предмет
            gameObject.SetActive(false);
        }
    }
}
