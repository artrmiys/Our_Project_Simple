using System.Collections;
using UnityEngine;
using TMPro;

public class NPCInteractable : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Коллайдер должен быть isTrigger. Игрок должен иметь тег Player.")]
    public string playerTag = "Player";
    public GameObject arrowIndicator;      // объект в Hierarchy (Image в World Space Canvas)
    [Tooltip("0 = не поворачивать; >0 = сглаживание поворота к камере (ед/с)")]
    public float arrowBillboardToCamera = 0f;

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;       // панель (Overlay Canvas)
    public TMP_Text dialogueText;          // TextMeshProUGUI
    [TextArea(3, 6)]
    public string[] lines;                 // реплики
    [Tooltip("Секунд на символ. 0 = без эффекта.")]
    public float typeSpeed = 0f;

    [Header("Input")]
    public KeyCode interactKey = KeyCode.Return;

    bool playerInRange;
    bool dialogueOpen;
    int lineIndex = -1;
    Coroutine typeRoutine;
    Camera cam; // кеш

    void Reset()
    {
        if (arrowIndicator == null && transform.childCount > 0)
            arrowIndicator = transform.GetChild(0).gameObject;
    }

    void Awake()
    {
        cam = Camera.main;
    }

    void Start()
    {
        if (arrowIndicator) arrowIndicator.SetActive(false);
        if (dialoguePanel) dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange) return;

        // стрелка видна, только когда диалог закрыт
        if (arrowIndicator) arrowIndicator.SetActive(!dialogueOpen);

        if (Input.GetKeyDown(interactKey))
        {
            if (!dialogueOpen) OpenDialogue();
            else AdvanceDialogue();
        }
    }

    void LateUpdate()
    {
        // Поворот стрелки к камере (стабильный вариант)
        if (arrowIndicator && arrowIndicator.activeSelf && arrowBillboardToCamera >= 0f && cam)
        {
            // Желаемая ориентация — «смотреть» туда же, куда и камера
            Quaternion target = Quaternion.LookRotation(cam.transform.forward, Vector3.up);
            if (arrowBillboardToCamera == 0f)
                arrowIndicator.transform.rotation = target;
            else
                arrowIndicator.transform.rotation = Quaternion.Slerp(
                    arrowIndicator.transform.rotation,
                    target,
                    arrowBillboardToCamera * Time.deltaTime
                );
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            if (arrowIndicator && !dialogueOpen) arrowIndicator.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            if (arrowIndicator) arrowIndicator.SetActive(false);
            if (dialogueOpen) CloseDialogue();
        }
    }

    void OpenDialogue()
    {
        // защита: нет текста — не открываем
        if (lines == null || lines.Length == 0 || dialogueText == null || dialoguePanel == null)
        {
            Debug.LogWarning("[NPCInteractable] Dialogue not configured (lines/panel/text).");
            return;
        }

        dialogueOpen = true;
        lineIndex = 0;
        dialoguePanel.SetActive(true);
        ShowCurrentLine();
    }

    void AdvanceDialogue()
    {
        if (!dialogueOpen) return;

        // если идёт машинописный эффект — докрутить до конца
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
            dialogueText.text = lines[lineIndex];
            return;
        }

        lineIndex++;
        if (lines == null || lineIndex >= lines.Length)
        {
            CloseDialogue();
        }
        else
        {
            ShowCurrentLine();
        }
    }

    void CloseDialogue()
    {
        dialogueOpen = false;
        lineIndex = -1;
        if (dialoguePanel) dialoguePanel.SetActive(false);
        // стрелка снова видна, если игрок ещё рядом
        if (arrowIndicator && playerInRange) arrowIndicator.SetActive(true);
    }

    void ShowCurrentLine()
    {
        if (dialogueText == null || lines == null || lineIndex < 0 || lineIndex >= lines.Length)
            return;

        if (typeSpeed > 0f)
        {
            if (typeRoutine != null) StopCoroutine(typeRoutine);
            typeRoutine = StartCoroutine(Typewriter(lines[lineIndex]));
        }
        else
        {
            dialogueText.text = lines[lineIndex];
        }
    }

    IEnumerator Typewriter(string fullText)
    {
        dialogueText.text = "";
        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        typeRoutine = null;
    }
}