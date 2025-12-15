using System.Collections.Generic;
using UnityEngine;

public class BossDeathMaterialSwap : MonoBehaviour

{
    [Header("Boss event source")]
    public Health bossHealth;                 // перетащи Health босса сюда
    public bool autoFindBossHealth = false;   // если включишь Ч попробует сам найти (если босс один)

    [Header("What to change")]
    public Material newMaterial;

    [Header("Targets (choose any combination)")]
    public List<Renderer> targetRenderers = new();     // пр€мые ссылки на Renderer
    public List<GameObject> targetRoots = new();       // объекты, у которых надо помен€ть материал (включа€ детей)

    [Header("Search options")]
    public bool includeInactiveChildren = true;        // UI/объекты могут быть выключены Ч всЄ равно найдЄм

    [Header("Apply options")]
    public bool useSharedMaterials = true;             // true = мен€ет sharedMaterials (без инстанса)
    public bool replaceAllSlots = true;               // true = заменить все слоты материалов
    public int slotIndex = 0;                         // если replaceAllSlots=false

    [Header("Debug")]
    public bool debugLogs = true;

    bool applied;
    readonly List<Renderer> cache = new();
    readonly HashSet<Renderer> seen = new();

    void Start()
    {
        if (!bossHealth && autoFindBossHealth)
        {
            var b = FindObjectOfType<BossAI>();
            if (b) bossHealth = b.GetComponent<Health>();
        }

        if (!bossHealth)
        {
            if (debugLogs) Debug.LogWarning("[SwapMaterials] bossHealth is NULL (no subscription).", this);
            return;
        }

        bossHealth.onDied.AddListener(Apply);
        if (debugLogs) Debug.Log("[SwapMaterials] Subscribed to boss onDied.", this);
    }

    void OnDisable()
    {
        if (bossHealth)
            bossHealth.onDied.RemoveListener(Apply);
    }

    void BuildCache()
    {
        cache.Clear();
        seen.Clear();

        // 1) direct renderers
        for (int i = 0; i < targetRenderers.Count; i++)
        {
            var r = targetRenderers[i];
            if (!r) continue;
            if (seen.Add(r)) cache.Add(r);
        }

        // 2) roots -> all renderers in children
        for (int i = 0; i < targetRoots.Count; i++)
        {
            var root = targetRoots[i];
            if (!root) continue;

            var rs = root.GetComponentsInChildren<Renderer>(includeInactiveChildren);
            for (int j = 0; j < rs.Length; j++)
            {
                var r = rs[j];
                if (!r) continue;
                if (seen.Add(r)) cache.Add(r);
            }
        }
    }

    public void Apply()
    {
        if (applied) return;
        applied = true;

        if (!newMaterial)
        {
            Debug.LogWarning("[SwapMaterials] newMaterial is NULL.", this);
            return;
        }

        BuildCache();

        if (debugLogs)
            Debug.Log($"[SwapMaterials] Apply() targets={cache.Count}", this);

        for (int i = 0; i < cache.Count; i++)
        {
            var r = cache[i];
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

    [ContextMenu("TEST Apply (without boss death)")]
    void TestApply()
    {
        applied = false;
        Apply();
    }
}
