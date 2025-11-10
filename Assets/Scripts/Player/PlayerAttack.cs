using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Spawn")]
    public Transform handPoint;

    [Header("Prefabs")]
    public GameObject whipPrefab;        // LMB
    public GameObject whipWidePrefab;    // RMB

    [Header("State")]
    public bool hasWhip = false;         // станет true после pickup

    [Header("Sound (optional)")]
    public AudioSource audioSource;
    public AudioClip whipSound;

    bool isAttacking = false;

    void Update()
    {
        // если не подобрали — не бьём
        if (!hasWhip) return;

        // ЛКМ
        if (Input.GetMouseButtonDown(0) && !isAttacking)
            StartCoroutine(SpawnWhip(whipPrefab, false));

        // ПКМ
        if (Input.GetMouseButtonDown(1) && !isAttacking)
            StartCoroutine(SpawnWhip(whipWidePrefab, true));
    }

    // это зовёт pickup
    public void CollectWhip()
    {
        hasWhip = true;   // ← вот тут должно быть true
    }

    IEnumerator SpawnWhip(GameObject prefab, bool isWide)
    {
        isAttacking = true;

        // небольшая задержка под анимацию
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

        // кд
        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
    }
}
