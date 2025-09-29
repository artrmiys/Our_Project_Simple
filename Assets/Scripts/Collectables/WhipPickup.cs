using UnityEngine;

public class WhipPickup : MonoBehaviour
{
    [Header("sound")]
    public AudioSource audioSource;   // �������� ����� (����� �� ���� �������)
    public AudioClip pickupSound;     // ���� �������

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ��� ���� ������
            PlayerAttack pa = other.GetComponent<PlayerAttack>();
            if (pa != null)
            {
                pa.CollectWhip();
            }

            // ���� �������
            if (audioSource && pickupSound)
                audioSource.PlayOneShot(pickupSound);

            // �������� ������
            gameObject.SetActive(false);
        }
    }
}
