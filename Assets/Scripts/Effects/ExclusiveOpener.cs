using System;
using System.Collections.Generic;
using UnityEngine;

public class ExclusiveOpener : MonoBehaviour
{
    [Header("Objects to hide/show")]
    [Tooltip("These objects will be hidden when any watch object is active.")]
    public List<GameObject> targets = new();

    [Header("Objects to watch")]
    [Tooltip("If any of these objects becomes active, targets will be hidden.")]
    public List<GameObject> watch = new();

    [Header("Behavior")]
    public bool useActiveInHierarchy = true;   // обычно лучше для UI
    public bool showTargetsWhenClear = true;   // вернуть targets когда watch все выключены
    public float checkEverySeconds = 0.05f;    // 0 = каждый кадр

    bool lastHideState;
    float t;

    void OnEnable()
    {
        ForceRefresh();
    }

    void Update()
    {
        if (checkEverySeconds > 0f)
        {
            t += Time.unscaledDeltaTime; // чтобы работало даже при паузе Time.timeScale = 0
            if (t < checkEverySeconds) return;
            t = 0f;
        }

        RefreshIfChanged();
    }

    public void ForceRefresh()
    {
        lastHideState = !ShouldHide(); // гарантируем применение
        RefreshIfChanged();
    }

    void RefreshIfChanged()
    {
        bool hide = ShouldHide();
        if (hide == lastHideState) return;

        lastHideState = hide;

        // hide targets
        for (int i = 0; i < targets.Count; i++)
        {
            var go = targets[i];
            if (!go) continue;

            if (hide) go.SetActive(false);
            else if (showTargetsWhenClear) go.SetActive(true);
        }
    }

    bool ShouldHide()
    {
        for (int i = 0; i < watch.Count; i++)
        {
            var w = watch[i];
            if (!w) continue;

            bool active = useActiveInHierarchy ? w.activeInHierarchy : w.activeSelf;
            if (active) return true;
        }
        return false;
    }
}