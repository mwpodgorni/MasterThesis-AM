using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseController : MonoBehaviour
{
    public VisualElement ui;
    public VisualElement pausePanel;
    public Button quitButton;
    public Button mainMenuButton;
    public Button resumeButton;

    private void OnEnable()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        pausePanel = ui.Q<VisualElement>("QuitPanel");
        quitButton = pausePanel.Q<Button>("QuitButton");
        mainMenuButton = pausePanel.Q<Button>("MenuButton");
        resumeButton = pausePanel.Q<Button>("ResumeButton");

        quitButton.clicked += QuitGame;
        mainMenuButton.clicked += LoadMainMenu;
        resumeButton.clicked += ResumeGame;

        pausePanel.style.display = DisplayStyle.None;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePausePanel();
        }
    }

    void TogglePausePanel()
    {
        pausePanel.style.display = pausePanel.style.display == DisplayStyle.None
            ? DisplayStyle.Flex
            : DisplayStyle.None;
    }

    void ResumeGame()
    {
        pausePanel.style.display = DisplayStyle.None;
    }

    public void LoadMainMenu()
    {
        StateManager.Instance.SetState(GameStage.StartingPoint);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
