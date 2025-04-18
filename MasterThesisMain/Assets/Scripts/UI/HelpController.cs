using System.Collections;
using System.Collections.Generic;
using TutorialData.Model;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class HelpController : MonoBehaviour
{
    public VisualElement ui;
    public VisualElement helpPanel;

    public Label helpTitle;
    public Label helpContent;
    public Button closeButton;
    public void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        helpPanel = ui.Q<VisualElement>("HelpPanel");
        helpTitle = ui.Q<Label>("HelpTitle");
        helpContent = ui.Q<Label>("HelpContent");
        closeButton = ui.Q<Button>("HelpCloseButton");
        closeButton.clicked += OnCloseButtonClicked;
        helpPanel.style.display = DisplayStyle.Flex;
        helpPanel.AddToClassList("help-hidden");
    }

    public void ShowHelp(string keyword)
    {
        if (StateManager.Instance.CurrentStage == GameStage.FirstWorkshopOpen)
        {
            StateManager.Instance.SetState(GameStage.FirstHelpOpen);
        }
        helpPanel.style.display = DisplayStyle.Flex;
        helpPanel.RemoveFromClassList("help-hidden");
        HelpText helpText = DataReader.Instance.GetHelpText(keyword);
        if (helpText != null)
        {
            helpTitle.text = helpText.title;
            helpContent.text = string.Join("\n\n", helpText.description);
        }
        else
        {
            helpTitle.text = "Help not found";
            helpContent.text = "No help available for this keyword.";
        }
    }
    public void OnCloseButtonClicked()
    {
        helpPanel.style.display = DisplayStyle.None;
        helpPanel.AddToClassList("help-hidden");
    }
}
