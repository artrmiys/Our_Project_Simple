using UnityEngine;

public class WhipPickup : MonoBehaviour
{
    [Header("pickup sound")]
    public AudioClip pickupSound;     // ���� �������

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // �������� ���� � ������
            PlayerAttack pa = other.GetComponent<PlayerAttack>();
            if (pa != null)
                pa.CollectWhip();

            // ����������� ���� ������� � ����� ��������
            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            // ������� �������
            gameObject.SetActive(false);
        }
    }
}
