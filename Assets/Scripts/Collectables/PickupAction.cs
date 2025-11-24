using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class PickupAction : MonoBehaviour
{
    [Header("Key condition")]
    [Tooltip("Require PrisonKey pickup (PrisonKeyPickup.IsTaken == true)")]
    public bool requirePrisonKey = true;

    [Header("Objects to hide on pickup")]
    public GameObject[] itemsToHide;      // these will be disabled

    [Header("Objects to show on pickup")]
    public GameObject[] itemsToShow;      // these will be enabled

    [Header("Animation")]
    public Animator targetAnimator;       // animator to control
    public string animTrigger;            // trigger to fire
    public string animBool;               // bool to set
    public bool boolValue = true;

    [Header("Explosion FX (optional)")]
    [Tooltip("Simple FX prefab (can be null if you use lightning storm)")]
    public GameObject explosionPrefab;    // particle / effect prefab
    public Transform explosionPoint;      // where to spawn (optional)
    public float explosionLifetime = 3f;  // destroy fx after

    [Header("Lightning storm (optional)")]
    [Tooltip("LightningEffect object with LightningStorm script")]
    public LightningStorm lightningStorm;     // reference to storm script
    public bool playStormOnPickup = true;     // play storm together with pickup

    [Header("Player & Level scene")]
    [Tooltip("Root object of Marion player to hide")]
    public GameObject playerToHide;       // Marion player root

    [Tooltip("Name of scene with level menu (must be in Build Settings)")]
    public string menuSceneName = "Menu";

    [Tooltip("Delay BEFORE hiding player (sec)")]
    public float playerHideDelay = 1f;    // 1 sec

    [Tooltip("Delay BEFORE loading menu scene (sec)")]
    public float menuDelay = 4f;          // 4 sec

    [Header("Hide this pickup")]
    public float hideDelay = 0.1f;        // delay before hiding this object

    [Header("Input")]
    public KeyCode interactKey = KeyCode.E; // key to press

    void Awake()
    {
        // make sure collider is a trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        // only player
        if (!other.CompareTag("Player"))
            return;

        // player stands inside and presses E
        if (Input.GetKeyDown(interactKey))
        {
            DoPickup();
        }
    }

    void DoPickup()
    {
        // 0) check key condition: need PrisonKey
        if (requirePrisonKey && !PrisonKeyPickup.IsTaken)
        {
            Debug.Log("[PickupAction] PrisonKey not taken yet. Action blocked.");
            return;
        }

        // 1) hide other items (e.g. prison bars)
        if (itemsToHide != null)
        {
            foreach (var go in itemsToHide)
            {
                if (go != null)
                    go.SetActive(false);
            }
        }

        // 2) show other items (camera, new objects, etc.)
        if (itemsToShow != null)
        {
            foreach (var go in itemsToShow)
            {
                if (go != null)
                    go.SetActive(true);
            }
        }

        // 3) play animation on target
        if (targetAnimator != null)
        {
            if (!string.IsNullOrEmpty(animTrigger))
                targetAnimator.SetTrigger(animTrigger);

            if (!string.IsNullOrEmpty(animBool))
                targetAnimator.SetBool(animBool, boolValue);
        }

        // 4) spawn simple explosion FX (optional)
        if (explosionPrefab != null)
        {
            Vector3 pos = explosionPoint ? explosionPoint.position : transform.position;
            Quaternion rot = Quaternion.identity;

            GameObject fx = Instantiate(explosionPrefab, pos, rot);
            if (explosionLifetime > 0f)
                Destroy(fx, explosionLifetime);
        }

        // 4b) play lightning storm (big blue lightning with smoke)
        if (playStormOnPickup && lightningStorm != null)
        {
            lightningStorm.PlayStorm();
        }

        // 5) schedule player hide and menu load
        if (playerHideDelay > 0f)
            Invoke(nameof(HidePlayer), playerHideDelay);
        else
            HidePlayer();

        if (menuDelay > 0f)
            Invoke(nameof(LoadMenuScene), menuDelay);
        else
            LoadMenuScene();

        // 6) hide this trigger object
        Invoke(nameof(HideSelf), hideDelay);

        // prevent multiple triggers
        enabled = false;
    }

    void HidePlayer()
    {
        if (playerToHide != null)
            playerToHide.SetActive(false);
    }

    void LoadMenuScene()
    {
        Time.timeScale = 1f; // normal time

        if (!string.IsNullOrEmpty(menuSceneName))
            SceneManager.LoadScene(menuSceneName);
    }

    void HideSelf()
    {
        gameObject.SetActive(false);
    }
}
