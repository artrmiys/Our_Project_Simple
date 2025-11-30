using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [Min(0.1f)] public float maxHP = 50f;
    [HideInInspector] public float currentHP;

    [Header("Popup")]
    public GameObject damagePopupPrefab;
    public Transform popupPoint;
    private readonly Color playerHitColor = Color.red;
    private readonly Color enemyHitColor = new Color(0.0745f, 1f, 0.0627f, 1f);

    [Header("Player death")]
    [Tooltip("UI object to show on player death")]
    public GameObject deathScreenObject;
    public float deathScreenTime = 2f;

    [Header("Destroy on death")]
    [Tooltip("Extra objects to remove with this one")]
    public GameObject[] extraObjectsToDestroy;

    [Header("Events")]
    public UnityEvent<float, float> onHealthChanged;
    public UnityEvent onDied;

    [Header("Player freeze on death")]
    [Tooltip("Tag used to detect player object")]
    public string playerTag = "Player";

    [Tooltip("Root object of the player (for disabling scripts)")]
    public Transform playerRoot;

    [Tooltip("Optional player rigidbody")]
    public Rigidbody playerRigidbody;

    [Tooltip("Extra scripts to disable on death")]
    public MonoBehaviour[] scriptsToDisableOnDeath;

    [Tooltip("Disable ALL MonoBehaviours on playerRoot hierarchy (except this Health)")]
    public bool disableAllPlayerScriptsOnDeath = true;

    // ----------------------------------------------
    // DAMAGE AUDIO
    // ----------------------------------------------
    [Header("Damage Audio")]
    public AudioSource damageAudioSource;   // AudioSource that plays damage sounds
    public AudioClip damageClip;            // Sound played when taking damage
    [Range(0f, 1f)] public float damageVolume = 1f; // Volume of damage sound


    void Awake()
    {
        currentHP = maxHP;

        // auto-setup for player
        if (CompareTag(playerTag))
        {
            if (!playerRoot)
                playerRoot = transform;

            if (!playerRigidbody && playerRoot)
                playerRigidbody = playerRoot.GetComponent<Rigidbody>();
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentHP <= 0f || amount <= 0f) return;

        currentHP = Mathf.Max(0f, currentHP - amount);

        // play damage sound
        PlayDamageSound(); // ← Added

        // popup
        if (damagePopupPrefab && popupPoint)
        {
            Vector3 pos = popupPoint.position + Vector3.up * 0.2f;
            GameObject popup = Instantiate(damagePopupPrefab, pos, Quaternion.identity);
            DamagePopup dp = popup.GetComponent<DamagePopup>();
            if (dp != null)
            {
                Color popupColor = CompareTag("Player") ? playerHitColor : enemyHitColor;
                dp.Setup(amount, popupColor);
            }
        }

        onHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0f)
            Die();
    }

    // -------------------------------------------------
    // METHOD: PLAYS THE DAMAGE SOUND
    // -------------------------------------------------
    void PlayDamageSound()
    {
        // play damage audio if everything is set
        if (damageAudioSource != null && damageClip != null)
        {
            damageAudioSource.PlayOneShot(damageClip, damageVolume);
        }
    }


    public void Heal(float amount)
    {
        if (currentHP <= 0f || amount <= 0f) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        onHealthChanged?.Invoke(currentHP, maxHP);
    }

    void Die()
    {
        onDied?.Invoke();
        Debug.Log($"{gameObject.name} died");

        if (CompareTag(playerTag))
        {
            FreezePlayerOnDeath();

            if (deathScreenObject != null)
                deathScreenObject.SetActive(true);

            GameObject go = new GameObject("DeathHandler");
            var handler = go.AddComponent<DeathHandler>();
            handler.delay = deathScreenTime;

            DestroyExtraObjects();
            Destroy(gameObject);
        }
        else
        {
            DestroyExtraObjects();
            Destroy(gameObject);
        }
    }

    void FreezePlayerOnDeath()
    {
        Input.ResetInputAxes();

        if (!playerRoot)
            return;

        if (!playerRigidbody)
            playerRigidbody = playerRoot.GetComponent<Rigidbody>();

        if (playerRigidbody != null)
        {
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.isKinematic = true;
            playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        if (scriptsToDisableOnDeath != null)
        {
            foreach (var mb in scriptsToDisableOnDeath)
            {
                if (mb != null)
                    mb.enabled = false;
            }
        }

        if (disableAllPlayerScriptsOnDeath && playerRoot != null)
        {
            var all = playerRoot.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in all)
            {
                if (mb == null || !mb.enabled) continue;
                if (mb == this) continue;

                mb.enabled = false;
            }
        }
    }

    void DestroyExtraObjects()
    {
        if (extraObjectsToDestroy == null) return;
        foreach (var obj in extraObjectsToDestroy)
        {
            if (obj != null)
                Destroy(obj);
        }
    }

    private class DeathHandler : MonoBehaviour
    {
        public float delay = 2f;

        void Start()
        {
            StartCoroutine(RestartRoutine());
        }

        System.Collections.IEnumerator RestartRoutine()
        {
            yield return new WaitForSeconds(delay);
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
            Destroy(gameObject);
        }
    }
}