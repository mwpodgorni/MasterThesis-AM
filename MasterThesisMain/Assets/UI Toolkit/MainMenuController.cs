using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
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

        optionsPanel = ui.Q<VisualElement>("OptionsPanel");
        optionsPanel.style.display = DisplayStyle.None;

        menu = ui.Q<VisualElement>("Menu");

    }
    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }
    private void OnOptionsClicked()
    {
        Debug.Log("OnOptionsClicked");
        optionsPanel.style.display = optionsPanel.style.display == DisplayStyle.None
             ? DisplayStyle.Flex
             : DisplayStyle.None;
    }
    private void OnOptionsClosed()
    {
        Debug.Log("OnOptionsClosed");
        optionsPanel.style.display = DisplayStyle.None;
    }
    private void OnPlayButtonClicked()
    {
        Debug.Log("Play Button Clicked");
        menu.style.display = DisplayStyle.None;
    }

}
