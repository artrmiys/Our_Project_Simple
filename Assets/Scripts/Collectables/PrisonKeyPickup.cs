using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PrisonKeyPickup : MonoBehaviour
{
    [Tooltip("Tag of player object")]
    public string playerTag = "Player";

    [Tooltip("Hide after pickup (sec)")]
    public float hideDelay = 0.01f;

    private void Awake()
    {
        // make sure it's a trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // only player
        if (!other.CompareTag(playerTag))
            return;

        // find inventory UI in scene
        InventoryUI inv = FindObjectOfType<InventoryUI>();
        if (inv != null)
        {
            // show prison key slot
            inv.AddItem("PrisonKey");
        }

        // just hide this pickup
        Invoke(nameof(HideMe), hideDelay);
    }

    void HideMe()
    {
        gameObject.SetActive(false);
    }
}
