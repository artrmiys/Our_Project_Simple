using UnityEngine;

public class TeleportOnT : MonoBehaviour
{
    [System.Serializable]
    public class TeleportEntry
    {
        public string name;                 // optional label in inspector
        public Transform targetPoint;       // where to teleport
        public KeyCode key = KeyCode.T;     // which key
        public bool matchRotation = true;   // copy rotation from target
        public bool useCtrlHeightOffset = true; // add CC height offset
    }

    [Header("Who to move")]
    public Transform player;                  // player root
    public string playerTag = "Player";
    public CharacterController playerCtrl;    // optional
    public Rigidbody playerRb;                // optional

    [Header("Teleport points")]
    public TeleportEntry[] teleports;         // list of points + keys

    [Header("Options")]
    public bool resetVelocity = true;         // zero RB velocity
    public bool log = true;                   // debug log

    void Awake()
    {
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) player = go.transform;
        }
        if (!playerCtrl && player) playerCtrl = player.GetComponent<CharacterController>();
        if (!playerRb && player) playerRb = player.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!player || teleports == null) return;

        // check all entries
        for (int i = 0; i < teleports.Length; i++)
        {
            var entry = teleports[i];
            if (entry == null || entry.targetPoint == null) continue;

            if (Input.GetKeyDown(entry.key))
            {
                TeleportNow(entry);
                break; // one teleport per frame is enough
            }
        }
    }

    public void TeleportNow(TeleportEntry entry)
    {
        if (!player || entry == null || entry.targetPoint == null) return;

        // disable CC during teleport
        bool hadCC = playerCtrl && playerCtrl.enabled;
        if (hadCC) playerCtrl.enabled = false;

        // compute target pos
        float offsetY = 0f;
        if (entry.useCtrlHeightOffset && playerCtrl)
            offsetY = playerCtrl.height * 0.5f;

        Vector3 newPos = entry.targetPoint.position + Vector3.up * offsetY;

        // move
        player.position = newPos;
        if (entry.matchRotation)
            player.rotation = entry.targetPoint.rotation;

        // reset physics
        if (resetVelocity && playerRb)
        {
            playerRb.velocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        // enable CC back
        if (hadCC) playerCtrl.enabled = true;

        if (log)
            Debug.Log($"[TeleportOnT] Teleported '{player.name}' to {newPos} via key {entry.key}");
    }

    // overload to allow calling from other scripts by index
    public void TeleportNow(int index)
    {
        if (teleports == null) return;
        if (index < 0 || index >= teleports.Length) return;
        TeleportNow(teleports[index]);
    }

    void OnDrawGizmosSelected()
    {
        if (teleports == null) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < teleports.Length; i++)
        {
            var entry = teleports[i];
            if (entry == null || entry.targetPoint == null) continue;

            Gizmos.DrawWireSphere(entry.targetPoint.position, 0.2f);

            if (player)
                Gizmos.DrawLine(player.position, entry.targetPoint.position);
        }
    }
}
