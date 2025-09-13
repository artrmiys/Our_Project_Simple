using UnityEngine;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    public Text interactionText;   
    public float rayDistance = 5f; 
    public RectTransform crosshair;

    


    void Update()
    {
       
        interactionText.gameObject.SetActive(false);

        Ray ray = Camera.main.ScreenPointToRay(crosshair.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            var interactable = hit.transform.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                interactionText.text = interactable.GetItemName();
                interactionText.gameObject.SetActive(true);
            }
        }
    }
}
