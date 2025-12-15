using UnityEngine;

public class BossAnimTest : MonoBehaviour
{
    Animator anim;

    void Awake() => anim = GetComponent<Animator>();

    void Update()
    {
        anim.SetFloat("Speed", 1f); // постоянно держим Walk
        if (Input.GetKeyDown(KeyCode.T))
            anim.SetTrigger("Attack");
    }
}