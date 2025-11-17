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

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float amount)
    {
        if (currentHP <= 0f || amount <= 0f) return;

        currentHP = Mathf.Max(0f, currentHP - amount);

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

        if (CompareTag("Player"))
        {
            // СБРОС ВВОДА С КЛАВЫ (WASD и т.п.), чтобы сразу перестало реагировать
            Input.ResetInputAxes();

            // show ui
            if (deathScreenObject != null)
                deathScreenObject.SetActive(true);

            // make temp runner
            GameObject go = new GameObject("DeathHandler");
            var handler = go.AddComponent<DeathHandler>();
            handler.delay = deathScreenTime;

            // remove extras
            DestroyExtraObjects();

            // remove player now
            Destroy(gameObject);
        }
        else
        {
            DestroyExtraObjects();
            Destroy(gameObject);
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

    // helper that survives player destroy
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
