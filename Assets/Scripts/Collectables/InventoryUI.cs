using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("slots")]
    public RawImage keySlot;
    public RawImage whipSlot;

    private bool hasKey = false;
    private bool hasWhip = false;

    void Start()
    {
        if (keySlot) keySlot.gameObject.SetActive(false);
        if (whipSlot) whipSlot.gameObject.SetActive(false);
    }

    public void AddItem(string itemName)
    {
        if (itemName == "Key" && !hasKey)
        {
            hasKey = true;
            if (keySlot) keySlot.gameObject.SetActive(true);
        }
        else if (itemName == "Whip" && !hasWhip)
        {
            hasWhip = true;
            if (whipSlot) whipSlot.gameObject.SetActive(true);
        }
    }
}
