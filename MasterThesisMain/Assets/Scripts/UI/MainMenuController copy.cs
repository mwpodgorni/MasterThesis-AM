using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
public class MainMenuControlle2 : MonoBehaviour
{
    [SerializeField] private SceneAsset stageOneScene;
    // public static MainMenuController Instance { get; private set; }
    public VisualElement ui;


    private void Awake()
    {
        // ui = GetComponent<UIDocument>().rootVisualElement;
        // if (Instance != null && Instance != this)
        // {
        //     Destroy(gameObject);
        //     return;
        // }
        // // Instance = this;
        // DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        // playButton = ui.Q<Button>("PlayButton");
        // playButton.clicked += OnPlayButtonClicked;
        // optionsButton = ui.Q<Button>("OptionsButton");
        // optionsButton.clicked += OnOptionsClicked;
        // quitButton = ui.Q<Button>("QuitButton");
        // quitButton.clicked += OnQuitButtonClicked;
        // optionsClosed = ui.Q<Button>("OptionsCloseButton");
        // optionsClosed.clicked += OnOptionsClosed;


        // menu = ui.Q<VisualElement>("Menu");
        // menu.style.display = DisplayStyle.Flex;
        // optionsPanel = ui.Q<VisualElement>("OptionsPanel");
        // optionsPanel.style.display = DisplayStyle.None;
        // optionsPanel.AddToClassList("opacity-none");



        // tutorialPanel = ui.Q<VisualElement>("TutorialPanel");
        // tutorialPanel.style.display = DisplayStyle.Flex;

        // miniGamePanel = ui.Q<VisualElement>("MiniGamePanel");
        // miniGamePanel.style.display = DisplayStyle.Flex;

        // helpPanel = ui.Q<VisualElement>("HelpPanel");
        // helpPanel.style.display = DisplayStyle.Flex;

        // TODO : remove this debug code
        // HideAllPanels();
        // menu.style.display = DisplayStyle.Flex;
        // menu.RemoveFromClassList("opacity-none");
        // menu.pickingMode = PickingMode.Position;
    }
    // public void HideAllPanels()
    // {
    //     menu.AddToClassList("opacity-none");
    //     menu.style.display = DisplayStyle.None;
    //     // menu.pickingMode = PickingMode.Ignore;
    //     tutorialPanel.AddToClassList("opacity-none");
    //     tutorialPanel.style.display = DisplayStyle.None;
    //     // tutorialPanel.pickingMode = PickingMode.Ignore;
    //     miniGamePanel.AddToClassList("opacity-none");
    //     miniGamePanel.style.display = DisplayStyle.None;
    //     // miniGamePanel.pickingMode = PickingMode.Ignore;
    //     optionsPanel.AddToClassList("opacity-none");
    //     optionsPanel.style.display = DisplayStyle.None;
    //     optionsPanel.pickingMode = PickingMode.Ignore;
    // }
    // private void OnQuitButtonClicked()
    // {
    //     Application.Quit();
    // }
    // private void OnOptionsClicked()
    // {
    //     Debug.Log("OnOptionsClicked");
    //     // HideAllPanels();
    //     optionsPanel.style.display = DisplayStyle.Flex;
    //     // optionsPanel.RemoveFromClassList("opacity-none");
    //     // optionsPanel.pickingMode = PickingMode.Position;
    // }
    // private void OnOptionsClosed()
    // {
    //     Debug.Log("OnOptionsClosed");
    //     optionsPanel.AddToClassList("opacity-none");
    //     optionsPanel.pickingMode = PickingMode.Ignore;
    //     // optionsPanel.style.display = DisplayStyle.None;
    //     // HideAllPanels();
    //     // menu.style.display = DisplayStyle.Flex;
    //     // menu.RemoveFromClassList("opacity-none");
    //     // menu.pickingMode = PickingMode.Position;
    // }
    // private void OnPlayButtonClicked()
    // {
    //     Debug.Log("Play Button Clicked");
    //     HideAllPanels();
    //     tutorialPanel.style.display = DisplayStyle.Flex;
    //     tutorialPanel.RemoveFromClassList("opacity-none");
    //     // tutorialPanel.pickingMode = PickingMode.Position;
    //     gameObject.GetComponent<TutorialController>().StartTutorial();
    // }
    // public void StartMiniGame()
    // {
    //     NetworkController.Instance.SetMiniGameObjective("Train a Neural Network");
    //     Debug.Log("Start Mini Game Button Clicked");
    //     // tutorialPanel.style.display = DisplayStyle.None;
    //     // miniGamePanel.style.display = DisplayStyle.Flex;
    // }
}
