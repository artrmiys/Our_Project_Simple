using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PrisonKeyPickup : MonoBehaviour
{
    // глобальный флаг: вз€ли ли мы этот ключ
    public static bool IsTaken { get; private set; } = false;

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

        // помечаем, что ключ вз€т
        IsTaken = true;

        // тут можешь оставить свою логику с InventoryUI, если нужно:
        // InventoryUI inv = FindObjectOfType<InventoryUI>();
        // if (inv != null) inv.AddItem("PrisonKey");

        // просто спр€чем объект ключа
        Invoke(nameof(HideMe), hideDelay);
    }

    void HideMe()
    {
        gameObject.SetActive(false);
    }
}
