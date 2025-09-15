using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class KeyPickup : MonoBehaviour
{
    [Header("Detection")]
    public string playerTag = "Player";     // тег игрока
    [Tooltip("Коллайдер этого объекта должен быть IsTrigger = ON")]
    public bool ensureIsTrigger = true;

    [Header("UI message")]
    public TMP_Text messageText;            // TextMeshProUGUI в Overlay-Canvas
    public GameObject messagePanel;         // опционально: панель диалога/баннер
    [TextArea] public string pickupMessage = "Ключ подобран";
    public float messageTime = 1.5f;        // сколько секунд показывать текст

    [Header("FX (optional)")]
    public AudioClip pickupSfx;             // звук подбора (опц.)
    public float destroyDelayAfterShow = 0.1f; // запас перед уничтожением

    Collider _col;
    Renderer[] _renderers;

    void Awake()
    {
        _col = GetComponent<Collider>();
        _renderers = GetComponentsInChildren<Renderer>(true);
        if (ensureIsTrigger && _col) _col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // 1) Показать сообщение
        if (messageText)
        {
            if (messagePanel) messagePanel.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(ShowMessageRoutine());
        }

        // 2) Проиграть звук (если указан)
        if (pickupSfx) AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        // 3) Спрятать визуал ключа и отключить повторные триггеры
        if (_col) _col.enabled = false;
        foreach (var r in _renderers) r.enabled = false;

        // 4) Уничтожить объект после показа сообщения
        float t = Mathf.Max(0f, messageTime) + destroyDelayAfterShow;
        Destroy(gameObject, t);
    }

    IEnumerator ShowMessageRoutine()
    {
        messageText.text = pickupMessage;
        yield return new WaitForSeconds(messageTime);

        // очистить/скрыть
        if (messagePanel) messagePanel.SetActive(false);
        else messageText.text = "";
    }
}
