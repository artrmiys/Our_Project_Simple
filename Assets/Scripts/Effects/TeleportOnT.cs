using UnityEngine;

public class TeleportOnT : MonoBehaviour
{
    [Header("Who to move")]
    public Transform player;                  // если пусто — найдём по тегу
    public string playerTag = "Player";
    public CharacterController playerCtrl;    // необязательно
    public Rigidbody playerRb;                // необязательно (если есть)

    [Header("Where to move")]
    public Transform targetPoint;             // точка телепорта

    [Header("Options")]
    public KeyCode key = KeyCode.T;
    public bool matchRotation = true;         // выровнять поворот как у target
    public bool useCtrlHeightOffset = true;   // добавить offset по высоте CC (как в SimplePortal)
    public bool resetVelocity = true;         // обнулить Rigidbody

    [Header("Debug")]
    public bool log = true;

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
        if (!player || !targetPoint) return;

        if (Input.GetKeyDown(key))
            TeleportNow();
    }

    public void TeleportNow()
    {
        if (!player || !targetPoint) return;

        // 1) отключаем CC на время телепорта
        bool hadCC = playerCtrl && playerCtrl.enabled;
        if (hadCC) playerCtrl.enabled = false;

        // 2) считаем позицию
        float offsetY = 0f;
        if (useCtrlHeightOffset && playerCtrl) offsetY = playerCtrl.height * 0.5f;

        Vector3 newPos = targetPoint.position + Vector3.up * offsetY;

        // 3) переносим
        player.position = newPos;
        if (matchRotation) player.rotation = targetPoint.rotation;

        // 4) чистим физику
        if (resetVelocity && playerRb)
        {
            playerRb.velocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        // 5) возвращаем CC
        if (hadCC) playerCtrl.enabled = true;

        if (log) Debug.Log($"[TeleportOnT] Teleported to {newPos}");
    }

    void OnDrawGizmosSelected()
    {
        if (!targetPoint) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(targetPoint.position, 0.2f);
        if (player)
        {
            Gizmos.DrawLine(player.position, targetPoint.position);
        }
    }
}
