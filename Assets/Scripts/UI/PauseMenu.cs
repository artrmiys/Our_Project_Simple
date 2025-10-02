using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;     // панель паузы
    [SerializeField] private Button resumeButton;       // кнопка Resume
    [SerializeField] private Button controlsButton;     // кнопка Show Controls
    [SerializeField] private Button exitButton;         // кнопка Exit to Menu
    [SerializeField] private GameObject controlsPanel;  // панель инструкций
    [SerializeField] private Button closeControlsButton;// кнопка закрытия Controls

    private bool isPaused = false;

    private void Start()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);

        // навешиваем кнопки
        resumeButton.onClick.AddListener(ResumeGame);
        controlsButton.onClick.AddListener(ShowControls);
        closeControlsButton.onClick.AddListener(HideControls);
        exitButton.onClick.AddListener(ExitToMenu);

        // добавим hover-анимацию
        AddHoverEffect(resumeButton);
        AddHoverEffect(controlsButton);
        AddHoverEffect(exitButton);
        AddHoverEffect(closeControlsButton);
    }

    private void Update()
    {
        // проверка Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    private void AddHoverEffect(Button btn)
    {
        var hover = btn.gameObject.AddComponent<HoverScale>();
        hover.scaleFactor = 1.3f;
        hover.speed = 10f;
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // стоп игра
        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);
        Time.timeScale = 1f; // продолжить игра
        isPaused = false;
    }

    public void ShowControls()
    {
        controlsPanel.SetActive(true);
    }

    public void HideControls()
    {
        controlsPanel.SetActive(false);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f; // вернуть время
        SceneManager.LoadScene("MainMenu");
    }
}
