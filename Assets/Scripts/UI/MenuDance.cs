using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MenuDance : MonoBehaviour
{
    public AnimationClip danceClip;

    void Start()
    {
        var animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = AnimatorControllerForClip(danceClip);
    }

    RuntimeAnimatorController AnimatorControllerForClip(AnimationClip clip)
    {
        var controller = new AnimatorOverrideController();
        controller.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("EmptyController");
        controller["Empty"] = clip;
        return controller;
    }
}
