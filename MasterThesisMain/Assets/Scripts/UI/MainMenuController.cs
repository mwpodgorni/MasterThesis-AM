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
    public Button optionsButton;
    public Button quitButton;
    public Button optionsClosed;
    public VisualElement optionsPanel;

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
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        playButton = ui.Q<Button>("PlayButton");
        playButton.clicked += OnPlayButtonClicked;
        optionsButton = ui.Q<Button>("OptionsButton");
        optionsButton.clicked += OnOptionsClicked;
        quitButton = ui.Q<Button>("QuitButton");
        quitButton.clicked += OnQuitButtonClicked;
        optionsClosed = ui.Q<Button>("OptionsCloseButton");
        optionsClosed.clicked += OnOptionsClosed;


        menu = ui.Q<VisualElement>("Menu");
        menu.style.display = DisplayStyle.Flex;
        optionsPanel = ui.Q<VisualElement>("OptionsPanel");
        optionsPanel.style.display = DisplayStyle.Flex;
        optionsPanel.AddToClassList("settings-hidden");
    }
    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }
    private void OnOptionsClicked()
    {
        optionsPanel.RemoveFromClassList("settings-hidden");
    }
    private void OnOptionsClosed()
    {
        optionsPanel.AddToClassList("settings-hidden");
    }
    private void OnPlayButtonClicked()
    {
        Debug.Log("Play Button Clicked");
        // gameObject.GetComponent<TutorialController>().StartTutorial();
        SceneManager.LoadScene(stageOneScene.name);
    }
}
