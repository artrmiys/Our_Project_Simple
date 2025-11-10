using UnityEngine;

public class PickupAction : MonoBehaviour
{
    [Header("Objects to hide on pickup")]
    public GameObject[] itemsToHide;      // these will be disabled

    [Header("Objects to show on pickup")]
    public GameObject[] itemsToShow;      // these will be enabled

    [Header("Animation")]
    public Animator targetAnimator;       // animator to control
    public string animTrigger;            // trigger to fire
    public string animBool;               // bool to set
    public bool boolValue = true;

    [Header("Hide this pickup")]
    public float hideDelay = 0.1f;        // delay before hiding this object

    private void OnTriggerEnter(Collider other)
    {
        // only player
        if (!other.CompareTag("Player"))
            return;

        // 1) hide other items
        if (itemsToHide != null)
        {
            foreach (var go in itemsToHide)
            {
                if (go != null)
                    go.SetActive(false);
            }
        }

        // 2) show other items
        if (itemsToShow != null)
        {
            foreach (var go in itemsToShow)
            {
                if (go != null)
                    go.SetActive(true);
            }
        }

        // 3) play animation
        if (targetAnimator != null)
        {
            if (!string.IsNullOrEmpty(animTrigger))
                targetAnimator.SetTrigger(animTrigger);

            if (!string.IsNullOrEmpty(animBool))
                targetAnimator.SetBool(animBool, boolValue);
        }

        // 4) hide this pickup
        Invoke(nameof(HideSelf), hideDelay);
    }

    void HideSelf()
    {
        gameObject.SetActive(false);
    }
}
