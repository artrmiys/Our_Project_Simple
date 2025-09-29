using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InventoryPickup : MonoBehaviour
{
    [Header("settings")]
    public string itemName;       // "Key" или "Whip"
    public AudioClip pickupSound; // звук подбора

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // добавить в инвентарь
        InventoryUI inv = FindObjectOfType<InventoryUI>();
        if (inv != null)
            inv.AddItem(itemName);

        // звук
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        // скрыть объект
        gameObject.SetActive(false);
    }
}
