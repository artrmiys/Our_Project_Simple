using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Spawn")]
    public Transform handPoint;

    [Header("Prefabs")]
    public GameObject whipPrefab;        // LMB basic
    public GameObject whipWidePrefab;    // RMB wide

    [Header("State")]
    public bool hasWhipBasic = false;    // after basic pickup
    public bool hasWhipWide = false;     // after wide pickup

    // legacy flag for old scripts (PlayerMovement etc.)
    [HideInInspector] public bool hasWhip = false;  // true if any whip

    [Header("Sound (optional)")]
    public AudioSource audioSource;
    public AudioClip whipSound;

    bool isAttacking = false;

    void Update()
    {
        // keep legacy flag in sync
        hasWhip = hasWhipBasic || hasWhipWide;

        if (isAttacking) return;

        // LMB – basic whip
        if (hasWhipBasic && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(SpawnWhip(whipPrefab, false));
        }

        // RMB – wide whip
        if (hasWhipWide && Input.GetMouseButtonDown(1))
        {
            StartCoroutine(SpawnWhip(whipWidePrefab, true));
        }
    }

    // basic pickup (LMB)
    public void CollectWhipBasic()
    {
        hasWhipBasic = true;
        hasWhip = true;
    }

    // wide pickup (RMB)
    public void CollectWhipWide()
    {
        hasWhipWide = true;
        hasWhip = true;
    }

    // старый метод, если где-то уже вызывается
    public void CollectWhip()
    {
        hasWhipBasic = true;
        hasWhip = true;
    }

    IEnumerator SpawnWhip(GameObject prefab, bool isWide)
    {
        isAttacking = true;

        // small delay for anim
        yield return new WaitForSeconds(0.25f);

        if (prefab && handPoint)
        {
            GameObject go = Instantiate(prefab, handPoint.position, handPoint.rotation);
            go.transform.SetParent(handPoint, true);

            if (isWide)
            {
                var wide = go.GetComponent<WhipWide>();
                if (wide != null)
                    wide.SetOwner(transform);
            }
        }

        if (audioSource && whipSound)
            audioSource.PlayOneShot(whipSound);

        // cooldown
        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
    }
}
