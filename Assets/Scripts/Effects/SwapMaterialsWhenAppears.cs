using System.Collections.Generic;
using UnityEngine;

public class SwapMaterialsWhenAppears : MonoBehaviour
{
    [Header("Watch (the object that becomes visible)")]
    public GameObject watchRoot;

    [Tooltip("If true, require watchRoot.activeInHierarchy == true")]
    public bool requireActiveInHierarchy = false;

    [Tooltip("If true, 'appeared' when any Renderer.enabled becomes true")]
    public bool detectRendererEnabled = true;

    [Tooltip("If true, also treat alpha > threshold as appeared (checks _BaseColor/_Color)")]
    public bool detectAlpha = false;
    [Range(0f, 1f)] public float alphaThreshold = 0.05f;

    [Tooltip("Include inactive children when searching renderers")]
    public bool includeInactiveChildren = true;

    [Header("Targets to swap materials on")]
    public List<Renderer> targetRenderers = new();
    public List<GameObject> targetRoots = new(); // auto-find renderers in children

    [Header("Material swap")]
    public Material newMaterial;
    public bool useSharedMaterials = true;
    public bool replaceAllSlots = true;
    public int slotIndex = 0;

    [Header("Timing")]
    public float checkEverySeconds = 0.05f;
    public bool fireOnce = true;

    [Header("Debug")]
    public bool debugLogs = false;

    bool fired;
    float nextCheck;
    readonly List<Renderer> watchRenderers = new();
    readonly List<Renderer> cacheTargets = new();
    readonly HashSet<Renderer> seen = new();

    void Awake()
    {
        RefreshWatchRenderers();
        BuildTargetCache();
    }

    void Update()
    {
        if (fireOnce && fired) return;

        if (Time.unscaledTime < nextCheck) return;
        nextCheck = Time.unscaledTime + Mathf.Max(0.01f, checkEverySeconds);

        if (HasAppeared())
        {
            if (debugLogs) Debug.Log("[SwapMaterialsWhenAppears] Appeared -> apply material", this);
            Apply();
            fired = true;
        }
    }

    public void RefreshWatchRenderers()
    {
        watchRenderers.Clear();
        if (!watchRoot) return;

        var rs = watchRoot.GetComponentsInChildren<Renderer>(includeInactiveChildren);
        for (int i = 0; i < rs.Length; i++)
            if (rs[i]) watchRenderers.Add(rs[i]);
    }

    void BuildTargetCache()
    {
        cacheTargets.Clear();
        seen.Clear();

        for (int i = 0; i < targetRenderers.Count; i++)
        {
            var r = targetRenderers[i];
            if (!r) continue;
            if (seen.Add(r)) cacheTargets.Add(r);
        }

        for (int i = 0; i < targetRoots.Count; i++)
        {
            var root = targetRoots[i];
            if (!root) continue;

            var rs = root.GetComponentsInChildren<Renderer>(true);
            for (int j = 0; j < rs.Length; j++)
            {
                var r = rs[j];
                if (!r) continue;
                if (seen.Add(r)) cacheTargets.Add(r);
            }
        }
    }

    bool HasAppeared()
    {
        if (!watchRoot) return false;

        if (requireActiveInHierarchy && !watchRoot.activeInHierarchy)
            return false;

        if (detectRendererEnabled)
        {
            for (int i = 0; i < watchRenderers.Count; i++)
            {
                var r = watchRenderers[i];
                if (!r) continue;
                if (r.enabled && r.gameObject.activeInHierarchy) return true;
            }
        }

        if (detectAlpha)
        {
            for (int i = 0; i < watchRenderers.Count; i++)
            {
                var r = watchRenderers[i];
                if (!r) continue;
                var mat = useSharedMaterials ? r.sharedMaterial : r.material;
                if (!mat) continue;

                if (TryGetAlpha(mat, out float a) && a > alphaThreshold)
                    return true;
            }
        }

        return false;
    }

    bool TryGetAlpha(Material mat, out float a)
    {
        a = 1f;
        if (!mat) return false;

        if (mat.HasProperty("_BaseColor"))
        {
            a = mat.GetColor("_BaseColor").a;
            return true;
        }
        if (mat.HasProperty("_Color"))
        {
            a = mat.GetColor("_Color").a;
            return true;
        }
        return false;
    }

    public void Apply()
    {
        if (!newMaterial)
        {
            Debug.LogWarning("[SwapMaterialsWhenAppears] newMaterial is not set.", this);
            return;
        }

        if (cacheTargets.Count == 0) BuildTargetCache();

        for (int i = 0; i < cacheTargets.Count; i++)
        {
            var r = cacheTargets[i];
            if (!r) continue;

            var mats = useSharedMaterials ? r.sharedMaterials : r.materials;
            if (mats == null || mats.Length == 0) continue;

            if (replaceAllSlots)
            {
                for (int m = 0; m < mats.Length; m++)
                    mats[m] = newMaterial;
            }
            else
            {
                int idx = Mathf.Clamp(slotIndex, 0, mats.Length - 1);
                mats[idx] = newMaterial;
            }

            if (useSharedMaterials) r.sharedMaterials = mats;
            else r.materials = mats;
        }
    }

    [ContextMenu("TEST: Apply Now")]
    void TestApplyNow() => Apply();
}
