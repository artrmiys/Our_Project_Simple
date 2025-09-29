using UnityEngine;

public class Portal : MonoBehaviour
{
    public Transform target;                // other portal
    public CharacterController playerCtrl;  // player ctrl
    public float cooldown = 0.5f;           // tp delay
    public GameObject needItem;             // item to check
    public GameObject appearObj;            // obj appear

    float lastTP = -999f;
    bool unlocked = false;

    void Update()
    {
        // unlock if item gone
        if (!unlocked && needItem == null)
        {
            unlocked = true;
            if (appearObj) appearObj.SetActive(true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // check player
        if (unlocked && other.CompareTag("Player") && Time.time - lastTP > cooldown)
        {
            Transform player = other.transform;

            // get root
            if (!player.GetComponent<CharacterController>() && playerCtrl)
                player = playerCtrl.transform;

            // off ctrl
            if (playerCtrl) playerCtrl.enabled = false;

            // set pos+rot with offset
            float offsetY = playerCtrl ? playerCtrl.height * 0.5f : 1f;
            Vector3 newPos = target.position + Vector3.up * offsetY;
            player.position = newPos;
            player.rotation = target.rotation;

            // on ctrl
            if (playerCtrl) playerCtrl.enabled = true;

            // cam shake
            if (CameraShake.Instance) CameraShake.Instance.Shake();

            // sync cd
            lastTP = Time.time;
            Portal tpTarget = target.GetComponent<Portal>();
            if (tpTarget) tpTarget.lastTP = Time.time;
        }
    }
}
