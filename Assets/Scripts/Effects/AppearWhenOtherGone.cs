using UnityEngine;

public class ShowWhenOtherGone : MonoBehaviour
{
    [Header("Watch this")]
    public GameObject target;       // этот объект исчезает

    [Header("Show this")]
    public GameObject objectToShow; // этот нужно включить

    private bool done = false;

    void Start()
    {
        // на всякий случай прячем второй на старте
        if (objectToShow != null)
            objectToShow.SetActive(false);
    }

    void Update()
    {
        if (done) return;

        // 1) target == null → его уничтожили (Destroy)
        // 2) !target.activeInHierarchy → его выключили SetActive(false)
        if (target == null || !target.activeInHierarchy)
        {
            if (objectToShow != null)
                objectToShow.SetActive(true);

            done = true; // чтобы не крутить это каждый кадр
        }
    }
}
