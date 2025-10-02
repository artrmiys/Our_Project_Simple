using UnityEngine;
using System.Collections;

public class SimpleInteraction : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public Collider targetCollider;   // collider of object
    public RectTransform symbolIcon;
    public GameObject textCanvas;
    public GameObject worldObject;
    public Renderer worldRenderer;

    [Header("Settings")]
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Symbol move")]
    public float floatSpeed = 2f;
    public float floatHeight = 10f;

    [Header("World object fade")]
    public float fadeSpeed = 2f;

    private bool isOpen = false;
    private Vector3 symbolStartPos;
    private float targetAlpha = 0f;

    void Start()
    {
        if (symbolIcon != null)
        {
            symbolStartPos = symbolIcon.anchoredPosition;
            symbolIcon.gameObject.SetActive(false);
        }

        if (textCanvas != null)
            textCanvas.SetActive(false);

        if (worldObject != null && worldRenderer == null)
            worldObject.SetActive(false);

        if (worldRenderer != null)
            SetAlpha(0f);
    }

    void Update()
    {
        if (!isOpen)
        {
            float dist = GetDistanceToCollider();

            if (dist <= interactDistance)
            {
                // === SYMBOL ===
                if (symbolIcon != null && !symbolIcon.gameObject.activeSelf)
                    symbolIcon.gameObject.SetActive(true);

                if (symbolIcon != null)
                {
                    float offset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
                    symbolIcon.anchoredPosition = symbolStartPos + new Vector3(0, offset, 0);
                }

                if (Input.GetKeyDown(interactKey))
                    OpenText();

                // === WORLD OBJ ===
                if (worldObject != null && worldRenderer == null)
                    worldObject.SetActive(true);

                if (worldRenderer != null)
                    targetAlpha = 1f;
            }
            else
            {
                if (symbolIcon != null && symbolIcon.gameObject.activeSelf)
                    symbolIcon.gameObject.SetActive(false);

                if (worldObject != null && worldRenderer == null)
                    worldObject.SetActive(false);

                if (worldRenderer != null)
                    targetAlpha = 0f;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E))
                CloseText();
        }

        // === FADE WORLD OBJ ===
        if (worldRenderer != null)
        {
            Color c = worldRenderer.material.color;
            float newAlpha = Mathf.Lerp(c.a, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
            SetAlpha(newAlpha);
        }
    }

    float GetDistanceToCollider()
    {
        if (targetCollider != null)
        {
            Vector3 closest = targetCollider.ClosestPoint(player.position);
            return Vector3.Distance(player.position, closest);
        }
        else
        {
            return Vector3.Distance(player.position, transform.position);
        }
    }

    void OpenText()
    {
        isOpen = true;

        if (symbolIcon != null) symbolIcon.gameObject.SetActive(false);
        if (textCanvas != null) textCanvas.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseText()
    {
        isOpen = false;

        if (textCanvas != null) textCanvas.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void SetAlpha(float a)
    {
        if (worldRenderer != null)
        {
            Color c = worldRenderer.material.color;
            c.a = a;
            worldRenderer.material.color = c;
        }
    }
}
