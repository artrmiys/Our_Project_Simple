using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;     // ������ �����
    [SerializeField] private Button resumeButton;       // ������ Resume
    [SerializeField] private Button controlsButton;     // ������ Show Controls
    [SerializeField] private Button exitButton;         // ������ Exit to Menu
    [SerializeField] private GameObject controlsPanel;  // ������ ����������
    [SerializeField] private Button closeControlsButton;// ������ �������� Controls

    private bool isPaused = false;

    private void Start()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);

        // ���������� ������
        resumeButton.onClick.AddListener(ResumeGame);
        controlsButton.onClick.AddListener(ShowControls);
        closeControlsButton.onClick.AddListener(HideControls);
        exitButton.onClick.AddListener(ExitToMenu);

        // ������� hover-��������
        AddHoverEffect(resumeButton);
        AddHoverEffect(controlsButton);
        AddHoverEffect(exitButton);
        AddHoverEffect(closeControlsButton);
    }

    private void Update()
    {
        // �������� Esc
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
        Time.timeScale = 0f; // ���� ����
        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);
        Time.timeScale = 1f; // ���������� ����
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
        Time.timeScale = 1f; // ������� �����
        SceneManager.LoadScene("MainMenu");
    }
}
