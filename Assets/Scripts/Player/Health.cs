using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

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
    public GameObject deathScreenObject;
    public float deathScreenTime = 2f;

    [Header("Destroy on death")]
    public GameObject[] extraObjectsToDestroy;

    [Header("Events")]
    public UnityEvent<float, float> onHealthChanged;
    public UnityEvent onDied;

    [Header("Player freeze on death")]
    public string playerTag = "Player";
    public Transform playerRoot;
    public Rigidbody playerRigidbody;
    public MonoBehaviour[] scriptsToDisableOnDeath;
    public bool disableAllPlayerScriptsOnDeath = true;

    [Header("Damage Audio")]
    public AudioSource damageAudioSource;
    public AudioClip damageClip;
    [Range(0f, 1f)] public float damageVolume = 1f;

    [Header("Enemy death (animation + delay destroy)")]
    public float enemyDeathDelay = 2.9f;
    public bool enemyUseUnscaledTime = false;
    public bool enemyDisableCollidersOnDeath = true;
    public bool enemyDisableNavMeshOnDeath = true;
    public bool enemyDisableEnemyAIOnDeath = true;

    [Header("Enemy corpse")]
    public float corpseStayTime = 4f;
    public bool freezePoseAfterDeath = true;
    public bool freezeByDisablingAnimator = true;

    public void RestoreToFull()
    {
        if (currentHP <= 0f) return;
        currentHP = maxHP;
        onHealthChanged?.Invoke(currentHP, maxHP);
    }


    void Awake()
    {
        currentHP = maxHP;

        if (CompareTag(playerTag))
        {
            if (!playerRoot) playerRoot = transform;
            if (!playerRigidbody && playerRoot) playerRigidbody = playerRoot.GetComponent<Rigidbody>();
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentHP <= 0f || amount <= 0f) return;

        currentHP = Mathf.Max(0f, currentHP - amount);
        PlayDamageSound();

        if (damagePopupPrefab && popupPoint)
        {
            Vector3 pos = popupPoint.position + Vector3.up * 0.2f;
            GameObject popup = Instantiate(damagePopupPrefab, pos, Quaternion.identity);
            DamagePopup dp = popup.GetComponent<DamagePopup>();
            if (dp != null)
            {
                Color popupColor = CompareTag(playerTag) ? playerHitColor : enemyHitColor;
                dp.Setup(amount, popupColor);
            }
        }

        onHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0f)
            Die();
    }

    void PlayDamageSound()
    {
        if (damageAudioSource != null && damageClip != null)
            damageAudioSource.PlayOneShot(damageClip, damageVolume);
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
            StartCoroutine(EnemyDeathRoutine());
        }
    }

    IEnumerator EnemyDeathRoutine()
    {
        // --- stop BossAI if exists ---
        var boss = GetComponent<BossAI>();
        if (boss != null)
        {
            boss.OnDeath();
            boss.enabled = false;
        }

        // --- stop EnemyAI if exists ---
        if (enemyDisableEnemyAIOnDeath)
        {
            var ai = GetComponent<EnemyAI>();
            if (ai != null) ai.enabled = false;
        }

        // --- stop NavMeshAgent ---
        if (enemyDisableNavMeshOnDeath)
        {
            var agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.enabled = false;
            }
        }

        // --- disable colliders ---
        if (enemyDisableCollidersOnDeath)
        {
            foreach (var col in GetComponentsInChildren<Collider>(true))
                col.enabled = false;
        }

        Animator anim = GetComponentInChildren<Animator>();
        float wait = enemyDeathDelay;

        if (anim != null)
        {
            // clear motion
            if (HasParam(anim, "Attack", AnimatorControllerParameterType.Trigger))
                anim.ResetTrigger("Attack");

            if (HasParam(anim, "Speed", AnimatorControllerParameterType.Float))
                anim.SetFloat("Speed", 0f);

            if (HasParam(anim, "isChasing", AnimatorControllerParameterType.Bool))
                anim.SetBool("isChasing", false);

            // IMPORTANT: set BOTH (your Boss uses Dead, others may use isDead)
            if (HasParam(anim, "isDead", AnimatorControllerParameterType.Bool))
                anim.SetBool("isDead", true);

            if (HasParam(anim, "Dead", AnimatorControllerParameterType.Bool))
                anim.SetBool("Dead", true);

            // force animator evaluate transitions immediately
            anim.Update(0f);
            yield return null; // let it enter Death state

            // try to auto-get current death clip length
            float clipLen = GetCurrentClipLength(anim, 0);
            if (clipLen > 0.05f)
                wait = clipLen;
        }

        if (enemyUseUnscaledTime)
            yield return new WaitForSecondsRealtime(wait);
        else
            yield return new WaitForSeconds(wait);

        if (freezePoseAfterDeath && anim != null)
        {
            if (freezeByDisablingAnimator) anim.enabled = false;
            else anim.speed = 0f;
        }

        if (corpseStayTime > 0f)
        {
            if (enemyUseUnscaledTime)
                yield return new WaitForSecondsRealtime(corpseStayTime);
            else
                yield return new WaitForSeconds(corpseStayTime);
        }

        DestroyExtraObjects();
        Destroy(gameObject);
    }

    static bool HasParam(Animator anim, string name, AnimatorControllerParameterType type)
    {
        if (!anim) return false;
        foreach (var p in anim.parameters)
            if (p.name == name && p.type == type) return true;
        return false;
    }

    static float GetCurrentClipLength(Animator anim, int layer)
    {
        if (!anim) return -1f;
        var infos = anim.GetCurrentAnimatorClipInfo(layer);
        float max = -1f;
        for (int i = 0; i < infos.Length; i++)
        {
            var c = infos[i].clip;
            if (c != null) max = Mathf.Max(max, c.length);
        }
        return max;
    }

    void FreezePlayerOnDeath()
    {
        Input.ResetInputAxes();
        if (!playerRoot) return;

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
                if (mb != null) mb.enabled = false;
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
            if (obj != null) Destroy(obj);
    }

    private class DeathHandler : MonoBehaviour
    {
        public float delay = 2f;

        void Start() => StartCoroutine(RestartRoutine());

        IEnumerator RestartRoutine()
        {
            yield return new WaitForSeconds(delay);
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
            Destroy(gameObject);
        }
    }
}
