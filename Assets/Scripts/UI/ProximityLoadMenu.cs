using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class ProximityLoadMenu : MonoBehaviour
{
    [Header("Who to watch")]
    public Transform player;                 // перетащи сюда игрока; если пусто — найдём по тегу
    public string playerTag = "Player";

    [Header("When to trigger")]
    public float triggerRadius = 1f;         // радиус срабатывания (м)
    public float delaySeconds = 5f;         // задержка перед загрузкой (реальное время)

    [Header("What to load")]
    public string menuSceneName = "Menu";    // имя сцены меню (как в Build Settings)

    [Header("Debug")]
    public bool debug = true;                // логи, линии и т.п.

    private bool triggered;
    private float triggerRadiusSqr;
    private float nextSearchTime;

    private void Awake()
    {
        // Если уже находимся в целевой сцене — не мешаем никому.
        if (SceneManager.GetActiveScene().name == menuSceneName)
        {
            if (debug) Debug.Log("[ProximityLoadMenu] Already in menu. Disabled.");
            enabled = false;
            return;
        }

        triggerRadiusSqr = triggerRadius * triggerRadius;

        // Попробуем найти игрока, если не задан.
        if (!player)
            TryFindPlayer();

        // Предупреждение, если сцены нет в Build Settings.
        if (debug && !Application.CanStreamedLevelBeLoaded(menuSceneName))
            Debug.LogWarning($"[ProximityLoadMenu] Scene '{menuSceneName}' is not in Build Settings.");
    }

    private void Update()
    {
        if (triggered) return;

        // Если потеряли ссылку на игрока — подыщем снова раз в 0.5с
        if (!player && Time.unscaledTime >= nextSearchTime)
        {
            TryFindPlayer();
            nextSearchTime = Time.unscaledTime + 0.5f;
        }
        if (!player) return;

        Vector3 d = player.position - transform.position;
        if (debug) Debug.DrawLine(transform.position, player.position, Color.yellow);

        if (d.sqrMagnitude <= triggerRadiusSqr)
        {
            triggered = true;
            if (debug) Debug.Log($"[ProximityLoadMenu] Player within {triggerRadius:F2} m. Loading in {delaySeconds}s.");
            StartCoroutine(LoadMenuAfterDelayRealtime());
        }
    }

    private IEnumerator LoadMenuAfterDelayRealtime()
    {
        // Ждём реальное время, чтобы сработало даже на паузе.
        float end = Time.unscaledTime + delaySeconds;
        while (Time.unscaledTime < end)
            yield return null;

        // Приводим игру в нормальное состояние перед переходом в меню.
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(menuSceneName);
    }

    private void TryFindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go) player = go.transform;
        if (debug) Debug.Log("[ProximityLoadMenu] Player " + (player ? "found." : "NOT found."));
    }

    private void OnDrawGizmos()
    {
        // Прозрачная сфера радиуса, чтобы легко увидеть точку.
        Gizmos.color = new Color(1f, 0f, 0f, 0.30f);
        Gizmos.DrawSphere(transform.position, triggerRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }

    // === Автоконфиг меню (без лишних скриптов) ===
    // Сработает каждый раз после загрузки любой сцены.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureMenuUsability()
    {
        string scene = SceneManager.GetActiveScene().name;
        // Ничего лишнего: правим только сцену меню.
        if (string.IsNullOrEmpty(scene)) return;

        // Имя сцены меню может отличаться от дефолтного — возьмём из любого живого экземпляра (если есть).
        string targetName = "Menu";
        var inst = Object.FindObjectOfType<ProximityLoadMenu>();
        if (inst) targetName = inst.menuSceneName;

        if (scene != targetName) return;

        // На всякий случай — нормализуем время и курсор.
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Гарантируем EventSystem, чтобы кнопки работали.
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
        }
    }
}
