using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Slots")]
    public RawImage keySlot;         // UI slot for normal key
    public RawImage whipSlot;        // UI slot for whip
    public RawImage prisonKeySlot;   // UI slot for prison key (render texture)

    // flags
    bool hasKey = false;
    bool hasWhip = false;
    bool hasPrisonKey = false;

    void Start()
    {
        // hide all at start
        if (keySlot) keySlot.gameObject.SetActive(false);
        if (whipSlot) whipSlot.gameObject.SetActive(false);
        if (prisonKeySlot) prisonKeySlot.gameObject.SetActive(false);
    }

    // call from pickup: AddItem("Key"), AddItem("Whip"), AddItem("PrisonKey")
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
        else if (itemName == "PrisonKey" && !hasPrisonKey)
        {
            hasPrisonKey = true;
            if (prisonKeySlot) prisonKeySlot.gameObject.SetActive(true);
        }
    }
}
