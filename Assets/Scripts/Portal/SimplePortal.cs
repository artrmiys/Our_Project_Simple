using System.Collections;
using UnityEngine;
using TMPro;

public class SimplePortal : MonoBehaviour
{
    [Header("Teleport")]
    public Transform target;
    public CharacterController playerCtrl;
    public float cooldown = 0.5f;
    public GameObject needItem;
    public GameObject appearObj;

    [Header("Monologue (on arrival)")]
    public bool playMonologueOnArrival = false;
    public bool playOnce = true;
    public GameObject dialoguePanel; // Panel под Canvas
    public TMP_Text dialogueText;    // TextMeshProUGUI
    [TextArea(3, 6)] public string[] lines;
    public float typeSpeed = 0f;
    public float pauseBetweenLines = 1.0f;

    [Header("Debug")]
    public bool debugLogs = true;
    public KeyCode debugKey = KeyCode.M;

    float lastTP = -999f;
    bool unlocked = false;
    bool monologuePlayed = false;
    Coroutine monologueRoutine;

    void Start()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!unlocked && needItem == null)
        {
            unlocked = true;
            if (appearObj) appearObj.SetActive(true);
            if (debugLogs) Debug.Log("[Portal:" + name + "] Unlocked (needItem is null).");
        }

        if (Input.GetKeyDown(debugKey))
        {
            if (debugLogs) Debug.Log("[Portal:" + name + "] DEBUG key pressed. TryPlayMonologue()");
            TryPlayMonologue();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (unlocked && other.CompareTag("Player") && Time.time - lastTP > cooldown)
        {
            if (debugLogs) Debug.Log("[Portal:" + name + "] OnTriggerEnter by " + other.name + ". Teleporting to " + (target ? target.name : "NULL"));

            Transform player = other.transform;
            if (!player.GetComponent<CharacterController>() && playerCtrl)
                player = playerCtrl.transform;

            if (playerCtrl) playerCtrl.enabled = false;

            float offsetY = playerCtrl ? playerCtrl.height * 0.5f : 1f;
            Vector3 newPos = target.position + Vector3.up * offsetY;
            player.position = newPos;
            player.rotation = target.rotation;

            if (playerCtrl) playerCtrl.enabled = true;

            if (CameraShake.Instance) CameraShake.Instance.Shake();

            lastTP = Time.time;
            SimplePortal tpTarget = target.GetComponent<SimplePortal>();
            if (tpTarget) tpTarget.lastTP = Time.time;

            if (tpTarget)
            {
                if (debugLogs) Debug.Log("[Portal:" + name + "] Calling TryPlayMonologue() on receiver " + tpTarget.name);
                tpTarget.TryPlayMonologue();
            }
            else if (debugLogs) Debug.LogWarning("[Portal:" + name + "] target has NO SimplePortal component.");
        }
        else
        {
            if (debugLogs) Debug.Log("[Portal:" + name + "] OnTriggerEnter ignored. unlocked=" + unlocked +
                                     ", tagOK=" + other.CompareTag("Player") +
                                     ", cdOK=" + (Time.time - lastTP > cooldown));
        }
    }

    public void TryPlayMonologue()
    {
        if (!playMonologueOnArrival)
        {
            if (debugLogs) Debug.Log("[Portal:" + name + "] playMonologueOnArrival is FALSE.");
            return;
        }
        if (playOnce && monologuePlayed)
        {
            if (debugLogs) Debug.Log("[Portal:" + name + "] monologue already played.");
            return;
        }
        if (monologueRoutine != null)
        {
            if (debugLogs) Debug.Log("[Portal:" + name + "] monologue already running.");
            return;
        }

        monologueRoutine = StartCoroutine(PlayMonologueRoutine());
    }

    IEnumerator PlayMonologueRoutine()
    {
        monologuePlayed = true;

        if (dialoguePanel == null || dialogueText == null || lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[Portal:" + name + "] Monologue UI not configured. panel=" + dialoguePanel + ", text=" + dialogueText + ", lines=" + (lines == null ? 0 : lines.Length));
            monologueRoutine = null;
            yield break;
        }

        var parentCanvas = dialoguePanel.GetComponentInParent<Canvas>(true);
        if (parentCanvas && !parentCanvas.gameObject.activeInHierarchy)
        {
            parentCanvas.gameObject.SetActive(true);
            if (debugLogs) Debug.Log("[Portal:" + name + "] Parent Canvas was inactive — activating.");
        }

        var col = dialogueText.color; col.a = 1f; dialogueText.color = col;

        dialoguePanel.SetActive(true);
        if (debugLogs) Debug.Log("[Portal:" + name + "] Monologue SHOW. Lines=" + lines.Length);

        for (int i = 0; i < lines.Length; i++)
        {
            if (debugLogs) Debug.Log("[Portal:" + name + "] Line " + (i + 1) + "/" + lines.Length + ": " + lines[i]);
            yield return StartCoroutine(ShowLine(lines[i]));
            yield return new WaitForSeconds(pauseBetweenLines);
        }

        dialoguePanel.SetActive(false);
        if (debugLogs) Debug.Log("[Portal:" + name + "] Monologue HIDE.");

        monologueRoutine = null;
    }

    IEnumerator ShowLine(string fullText)
    {
        if (dialogueText == null) yield break;

        if (typeSpeed <= 0f)
        {
            dialogueText.text = fullText;
            yield break;
        }

        dialogueText.text = "";
        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
    }
}
