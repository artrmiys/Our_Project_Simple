using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Transform handPoint; 
    public GameObject whipPrefab;  

    private bool isAttacking = false;   

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking) 
        {
            StartCoroutine(SpawnWhip());
        }
    }

    IEnumerator SpawnWhip()
    {
        isAttacking = true;

        // waiting
        yield return new WaitForSeconds(0.25f);

        // create
        Instantiate(whipPrefab, handPoint.position, handPoint.rotation);

        // waiting
        yield return new WaitForSeconds(0.4f);

        isAttacking = false;
    }
}
