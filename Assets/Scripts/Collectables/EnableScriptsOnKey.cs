using System.Collections.Generic;
using UnityEngine;

public class EnableScriptsOnKey : MonoBehaviour
{
    [Header("Key")]
    public KeyCode key = KeyCode.E;          // key to enable logic

    [Header("Player & radius")]
    public Transform player;                 // player reference
    public string playerTag = "Player";      // tag to auto-find
    public float interactRadius = 3f;        // how close player must be

    [Header("Scope")]
    public bool includeChildren = true;      // include children components

    [Header("Scripts control")]
    [Tooltip("Scripts that will NOT be disabled (stay active)")]
    public MonoBehaviour[] scriptWhitelist;  // scripts to keep enabled

    [Tooltip("Only these scripts will be disabled (optional). Empty = auto all.")]
    public MonoBehaviour[] scriptBlacklist;  // if you want precise list

    [Header("Colliders")]
    public bool disableColliders = true;     // disable colliders before key

    [Tooltip("These colliders will NOT be disabled")]
    public Collider[] colliderWhitelist;     // colliders to keep active

    [Header("Visual hide")]
    [Tooltip("These objects will be hidden before key and shown on unlock")]
    public GameObject[] hideObjects;         // objects to hide visually

    [Header("Behaviour")]
    public bool disableSelfAfterUnlock = true;

    // for aura, etc.
    public bool IsUnlocked => unlocked;

    private bool unlocked = false;
    private List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();
    private List<Collider> disabledColliders = new List<Collider>();
    private List<GameObject> actuallyHidden = new List<GameObject>(); // was active, we hid it

    void Awake()
    {
        // auto-find player
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) player = p.transform;
        }

        // 1) disable scripts
        MonoBehaviour[] allScripts;
        if (includeChildren)
            allScripts = GetComponentsInChildren<MonoBehaviour>(true);
        else
            allScripts = GetComponents<MonoBehaviour>();

        foreach (var mb in allScripts)
        {
            if (mb == null) continue;
            if (mb == this) continue;                    // keep this controller

            // if whitelist contains mb -> skip (keep enabled)
            if (IsInScriptWhitelist(mb)) continue;

            // if blacklist is set — disable only these
            if (scriptBlacklist != null && scriptBlacklist.Length > 0)
            {
                if (!IsInScriptBlacklist(mb)) continue;
            }

            if (mb.enabled)
            {
                disabledScripts.Add(mb);
                mb.enabled = false;                      // logic off before key
            }
        }

        // 2) disable colliders
        if (disableColliders)
        {
            Collider[] allCols;
            if (includeChildren)
                allCols = GetComponentsInChildren<Collider>(true);
            else
                allCols = GetComponents<Collider>();

            foreach (var col in allCols)
            {
                if (col == null) continue;
                if (IsInColliderWhitelist(col)) continue;

                if (col.enabled)
                {
                    disabledColliders.Add(col);
                    col.enabled = false;                 // no collisions / triggers
                }
            }
        }

        // 3) hide visual objects
        if (hideObjects != null)
        {
            foreach (var go in hideObjects)
            {
                if (go == null) continue;

                if (go.activeSelf)
                {
                    actuallyHidden.Add(go);
                    go.SetActive(false);                 // hide mesh / UI / etc.
                }
            }
        }
    }

    void Update()
    {
        if (unlocked) return;

        // if no player assigned – can't interact by radius
        if (!player) return;

        // check distance
        float dist = Vector3.Distance(player.position, transform.position);
        if (dist > interactRadius) return;   // too far – ignore E for this object

        // player is close enough – now we listen to key
        if (Input.GetKeyDown(key))
        {
            Unlock();
        }
    }

    private void Unlock()
    {
        // enable scripts back
        foreach (var mb in disabledScripts)
        {
            if (mb != null)
                mb.enabled = true;
        }
        disabledScripts.Clear();

        // enable colliders back
        foreach (var col in disabledColliders)
        {
            if (col != null)
                col.enabled = true;
        }
        disabledColliders.Clear();

        // show hidden objects
        foreach (var go in actuallyHidden)
        {
            if (go != null)
                go.SetActive(true);
        }
        actuallyHidden.Clear();

        unlocked = true;

        if (disableSelfAfterUnlock)
            enabled = false;
    }

    private bool IsInScriptWhitelist(MonoBehaviour mb)
    {
        if (scriptWhitelist == null) return false;
        for (int i = 0; i < scriptWhitelist.Length; i++)
        {
            if (scriptWhitelist[i] == mb)
                return true;
        }
        return false;
    }

    private bool IsInScriptBlacklist(MonoBehaviour mb)
    {
        if (scriptBlacklist == null) return false;
        for (int i = 0; i < scriptBlacklist.Length; i++)
        {
            if (scriptBlacklist[i] == mb)
                return true;
        }
        return false;
    }

    private bool IsInColliderWhitelist(Collider col)
    {
        if (colliderWhitelist == null) return false;
        for (int i = 0; i < colliderWhitelist.Length; i++)
        {
            if (colliderWhitelist[i] == col)
                return true;
        }
        return false;
    }

    // just for editor visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
