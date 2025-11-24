using UnityEngine;

public class HideOnKey : MonoBehaviour
{
    [Header("Target to hide")]
    public GameObject target;        // object to hide
    public KeyCode key = KeyCode.E;  // key to press

    [Header("Player & radius")]
    public Transform player;         // player ref (optional)
    public string playerTag = "Player";
    public float interactRadius = 3f; // how close player must be

    void Awake()
    {
        // default target = this object
        if (target == null)
            target = gameObject;

        // auto-find player if missing
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) player = p.transform;
        }
    }

    void Update()
    {
        if (!player || !target) return;

        // check distance
        float dist = Vector3.Distance(player.position, transform.position);
        if (dist > interactRadius) return;   // too far – ignore key

        // player is close enough – now listen to key
        if (Input.GetKeyDown(key))
        {
            target.SetActive(false);
            // or Destroy(target); if you want
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
