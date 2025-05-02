using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private SceneAsset stageOneScene;
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
        // DontDestroyOnLoad(gameObject);
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
        // Debug.Log("Play Button Clicked");
        SceneManager.LoadScene(stageOneScene.name);
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
