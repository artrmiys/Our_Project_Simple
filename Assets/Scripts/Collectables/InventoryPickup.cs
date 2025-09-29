using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InventoryPickup : MonoBehaviour
{
    [Header("settings")]
    public string itemName;       // "Key" ��� "Whip"
    public AudioClip pickupSound; // ���� �������

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // �������� � ���������
        InventoryUI inv = FindObjectOfType<InventoryUI>();
        if (inv != null)
            inv.AddItem(itemName);

        // ����
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        // ������ ������
        gameObject.SetActive(false);
    }
}
