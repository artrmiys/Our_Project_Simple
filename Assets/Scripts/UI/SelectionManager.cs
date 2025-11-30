using UnityEngine;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    public Text interactionText;
    public float rayDistance = 5f;
    public RectTransform crosshair;

    void Update()
    {
        // Safety: If UI or camera is missing, avoid errors
        if (interactionText == null || crosshair == null)
            return;

        Camera cam = Camera.main;

        // If there is no main camera (e.g., during cutscenes)
        if (cam == null)
        {
            interactionText.gameObject.SetActive(false);
            return;
        }

        interactionText.gameObject.SetActive(false);

        // Ray point: convert crosshair UI position to screen point
        Ray ray = cam.ScreenPointToRay(crosshair.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            if (hit.transform == null || hit.transform.Equals(null))
                return; // object was destroyed during cast

            var interactable = hit.transform.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                interactionText.text = interactable.GetItemName();
                interactionText.gameObject.SetActive(true);
            }
        }
    }
}
