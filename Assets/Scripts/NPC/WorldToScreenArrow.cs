using UnityEngine;

public class WorldToScreenArrow : MonoBehaviour
{
    public Transform target;                 // NPC
    public Vector3 worldOffset = new Vector3(0, 2f, 0);
    public RectTransform uiArrow;            // стрелка в Overlay-канвасе
    Camera cam;

    void Awake() { cam = Camera.main; }

    void LateUpdate()
    {
        if (!target || !cam) return;
        var sp = cam.WorldToScreenPoint(target.position + worldOffset);
        bool inFront = sp.z > 0f;
        uiArrow.gameObject.SetActive(inFront);
        if (inFront) uiArrow.position = sp;
    }
}
