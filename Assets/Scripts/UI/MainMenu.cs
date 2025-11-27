using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button controlButton;
    [SerializeField] private Button prototypeButton;   // prototype level

    [SerializeField] private GameObject controlsPanel; // controls panel
    [SerializeField] private Button closeControlsButton;
    [SerializeField] private GameObject[] hideOnControls; // UI to hide with panel

    private void Start()
    {
        // ensure proper state for menu
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // button clicks
        startButton.onClick.AddListener(StartGame);
        exitButton.onClick.AddListener(ExitGame);
        controlButton.onClick.AddListener(ShowControls);
        closeControlsButton.onClick.AddListener(HideControls);
        prototypeButton.onClick.AddListener(StartPrototype);

        // hover effects
        AddHoverEffect(startButton);
        AddHoverEffect(exitButton);
        AddHoverEffect(controlButton);
        AddHoverEffect(closeControlsButton);
        AddHoverEffect(prototypeButton);

        controlsPanel.SetActive(false);
    }

    private void AddHoverEffect(Button btn)
    {
        var hover = btn.gameObject.AddComponent<HoverScale>();
        hover.scaleFactor = 1.3f;
        hover.speed = 10f;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level_1");
    }

    public void StartPrototype()
    {
        SceneManager.LoadScene("Prototype");
    }

    public void ExitGame()
    {
        Debug.Log("Game closed!");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ShowControls()
    {
        controlsPanel.SetActive(true);

        foreach (var obj in hideOnControls)
        {
            if (obj != null) obj.SetActive(false);
        }
    }

    public void HideControls()
    {
        controlsPanel.SetActive(false);

        foreach (var obj in hideOnControls)
        {
            if (obj != null) obj.SetActive(true);
        }
    }
}

// hover scale effect for buttons
public class HoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float scaleFactor = 1.3f;
    public float speed = 10f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * speed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * scaleFactor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }
}
