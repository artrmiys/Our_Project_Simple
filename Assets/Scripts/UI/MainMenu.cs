using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button controlButton;
    [SerializeField] private GameObject controlsPanel;   // panel ref
    [SerializeField] private Button closeControlsButton; // close ref
    [SerializeField] private GameObject[] hideOnControls; // objects to hide

    private void Start()
    {
        // button clicks
        startButton.onClick.AddListener(StartGame);
        exitButton.onClick.AddListener(ExitGame);
        controlButton.onClick.AddListener(ShowControls);
        closeControlsButton.onClick.AddListener(HideControls);

        // hover effect
        AddHoverEffect(startButton);
        AddHoverEffect(exitButton);
        AddHoverEffect(controlButton);
        AddHoverEffect(closeControlsButton);

        controlsPanel.SetActive(false); // hidden start
    }

    private void AddHoverEffect(Button btn)
    {
        var hover = btn.gameObject.AddComponent<HoverScale>();
        hover.scaleFactor = 1.3f; // size factor
        hover.speed = 10f;        // anim speed
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level_1"); // load scene
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
        controlsPanel.SetActive(true); // show panel

        // hide objects
        foreach (var obj in hideOnControls)
        {
            if (obj != null) obj.SetActive(false);
        }
    }

    public void HideControls()
    {
        controlsPanel.SetActive(false); // hide panel

        // show back objects
        foreach (var obj in hideOnControls)
        {
            if (obj != null) obj.SetActive(true);
        }
    }
}

// hover effect
public class HoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float scaleFactor = 1.3f; // size factor
    public float speed = 10f;        // anim speed

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * scaleFactor; // grow
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale; // reset
    }
}
