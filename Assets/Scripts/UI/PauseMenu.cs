using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;       // pause panel
    [SerializeField] private GameObject controlsPanel;    // ctrl panel
    [SerializeField] private Button resumeButton;         // btn resume
    [SerializeField] private Button controlsButton;       // btn ctrl
    [SerializeField] private Button closeControlsButton;  // btn close
    [SerializeField] private Button exitButton;           // btn exit

    private bool isPaused;

    void Start()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);

        resumeButton.onClick.AddListener(ResumeGame);
        controlsButton.onClick.AddListener(ShowControls);
        closeControlsButton.onClick.AddListener(HideControls);
        exitButton.onClick.AddListener(ExitToMenu);

        // add hover
        AddHover(resumeButton);
        AddHover(controlsButton);
        AddHover(closeControlsButton);
        AddHover(exitButton);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // hover add
    private void AddHover(Button btn)
    {
        var hover = btn.gameObject.AddComponent<HoverEffect>();
        hover.scaleFactor = 1.2f;
        hover.speed = 12f;
    }

    // game pause
    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        controlsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    // game resume
    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // show ctrl
    void ShowControls() => controlsPanel.SetActive(true);

    // hide ctrl
    void HideControls() => controlsPanel.SetActive(false);

    // exit menu
    void ExitToMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Menu");
    }
}

// hover code
public class HoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float scaleFactor = 1.2f; // size mul
    public float speed = 10f;        // move spd

    private Vector3 originalScale;   // orig size
    private Vector3 targetScale;     // next size

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData) => targetScale = originalScale * scaleFactor;
    public void OnPointerExit(PointerEventData eventData) => targetScale = originalScale;
}
