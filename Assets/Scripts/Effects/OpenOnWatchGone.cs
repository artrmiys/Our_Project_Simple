using System.Collections.Generic;
using UnityEngine;

public class OpenOnWatchGone : MonoBehaviour
{
    [Header("Boss Animator (drag the object that has Animator)")]
    public Animator bossAnimator;

    [Header("Animator Bool that turns on death")]
    public string deathBool = "isDead";

    [Header("What to destroy (optional). If null -> destroy THIS object")]
    public GameObject targetToDestroy;

    [Header("Timing")]
    public float checkEverySeconds = 0.05f;

    float nextCheck;
    bool fired;

    void Update()
    {
        if (fired) return;
        if (!bossAnimator) return;

        if (Time.unscaledTime < nextCheck) return;
        nextCheck = Time.unscaledTime + Mathf.Max(0.01f, checkEverySeconds);

        if (bossAnimator.GetBool(deathBool))
        {
            fired = true;
            Destroy(targetToDestroy ? targetToDestroy : gameObject);
        }
    }
}