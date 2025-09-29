using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("whip")]
    public Transform handPoint;
    public GameObject whipPrefab;

    [Header("pickup")]
    public bool hasWhip = false;   // будет true после подбора ниток

    [Header("sound")]
    public AudioSource audioSource;
    public AudioClip whipSound;

    private bool isAttacking = false;

    void Update()
    {
        if (hasWhip && Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(SpawnWhip());
        }
    }

    IEnumerator SpawnWhip()
    {
        isAttacking = true;

        // delay before spawn
        yield return new WaitForSeconds(0.25f);

        // create whip
        Instantiate(whipPrefab, handPoint.position, handPoint.rotation);

        // play sound
        if (audioSource && whipSound)
            audioSource.PlayOneShot(whipSound);

        // cooldown
        yield return new WaitForSeconds(0.4f);

        isAttacking = false;
    }

    // вызовется из WhipPickup
    public void CollectWhip()
    {
        hasWhip = true;
    }
}
