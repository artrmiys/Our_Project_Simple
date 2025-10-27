using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("whip")]
    public Transform handPoint;
    public GameObject whipPrefab;

    [Header("pickup")]
    [Tooltip("true only after pickup")]
    public bool hasWhip = false;   // ❗ must be false at start

    [Header("sound")]
    public AudioSource audioSource;
    public AudioClip whipSound;

    bool isAttacking = false;

    void Start()
    {
        // force disable at start
        hasWhip = false;
    }

    void Update()
    {
        // no whip — no attack
        if (!hasWhip) return;

        // mouse attack
        if (Input.GetMouseButtonDown(0) && !isAttacking)
            StartCoroutine(SpawnWhip());
    }

    IEnumerator SpawnWhip()
    {
        isAttacking = true;

        yield return new WaitForSeconds(0.25f);

        // create whip
        Instantiate(whipPrefab, handPoint.position, handPoint.rotation);

        // play sound
        if (audioSource && whipSound)
            audioSource.PlayOneShot(whipSound);

        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
    }

    // called when pickup triggered
    public void CollectWhip()
    {
        hasWhip = true;
        Debug.Log("Whip collected!"); 
    }
}
