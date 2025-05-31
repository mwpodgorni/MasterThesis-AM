using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance { get; private set; }
    public VisualElement ui;
    public Button playButton;
    public Button paperButton;
    public Button quitButton;
    public Button paperClosed;
    public VisualElement paperPanel;
    public VisualElement controlsPanel;
    public Button controlsButton;
    public Button controlsClosed;
    public Button surveyButton;
    public VisualElement menu;
    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void OnEnable()
    {
        playButton = ui.Q<Button>("PlayButton");
        playButton.clicked += OnPlayButtonClicked;
        paperButton = ui.Q<Button>("PaperButton");
        paperButton.clicked += OnPaperButtonClicked;
        quitButton = ui.Q<Button>("QuitButton");
        quitButton.clicked += OnQuitButtonClicked;
        paperClosed = ui.Q<Button>("PaperCloseButton");
        paperClosed.clicked += OnPaperClosed;
        controlsClosed = ui.Q<Button>("ControlsCloseButton");
        controlsClosed.clicked += OnControlsClosed;
        controlsButton = ui.Q<Button>("ControlsButton");
        controlsButton.clicked += OnControlsButtonClicked;


        menu = ui.Q<VisualElement>("Menu");
        menu.style.display = DisplayStyle.Flex;
        paperPanel = ui.Q<VisualElement>("PaperPanel");
        paperPanel.style.display = DisplayStyle.Flex;
        paperPanel.AddToClassList("panel-up");
        controlsPanel = ui.Q<VisualElement>("ControlsPanel");
        controlsPanel.style.display = DisplayStyle.Flex;
        controlsPanel.AddToClassList("panel-up");

        surveyButton = ui.Q<Button>("SurveyButton");
        surveyButton.clicked += OnSurveyButtonClicked;
    }
    public void OnSurveyButtonClicked()
    {
        if (ActivityTracker.Instance != null)
        {
            Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSffHAoJXE4RetOsmDZG0FycdND9QlhSbru142JqOCFz9zDUAQ/viewform?usp=pp_url&entry.978412280=" + ActivityTracker.Instance.GetSessionId());
        }
        Application.Quit();
    }
    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }
    private void OnPaperButtonClicked()
    {
        if (ActivityTracker.Instance != null)
        {
            ActivityTracker.Instance.StartTimer("PaperOpen");
        }
        paperPanel.RemoveFromClassList("panel-up");
    }
    private void OnPaperClosed()
    {
        if (ActivityTracker.Instance != null)
        {
            ActivityTracker.Instance.StopTimer("PaperOpen");
            { }
            paperPanel.AddToClassList("panel-up");
        }
    }
    private void OnPlayButtonClicked()
    {
        if (ActivityTracker.Instance != null)
        {
            ActivityTracker.Instance.StopTimer("PaperOpen");
        }
        SceneManager.LoadScene("StageOne");
    }
    private void OnControlsButtonClicked()
    {
        controlsPanel.RemoveFromClassList("panel-up");
    }
    private void OnControlsClosed()
    {
        controlsPanel.AddToClassList("panel-up");
    }
}
