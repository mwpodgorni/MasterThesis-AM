using System.Collections;
using System.Collections.Generic;
using TutorialData.Model;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
public class HelpController : MonoBehaviour
{
    VisualElement ui;
    VisualElement helpPanel;

    Label helpTitle;
    VisualElement helpImage;
    Label helpDescription;
    Label helpHighlights;
    Button closeButton;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        helpPanel = ui.Q<VisualElement>("HelpPanel");
        helpTitle = ui.Q<Label>("HelpTitle");
        helpImage = ui.Q<VisualElement>("HelpImage");
        helpDescription = ui.Q<Label>("HelpDescription");
        helpHighlights = ui.Q<Label>("HelpHighlights");
        closeButton = ui.Q<Button>("HelpCloseButton");

        closeButton.clicked += OnCloseButtonClicked;

        helpPanel.style.display = DisplayStyle.None;
        helpPanel.AddToClassList("help-hidden");
    }

    public void ShowHelp(string keyword)
    {
        if (ActivityTracker.Instance != null)
        {
            ActivityTracker.Instance.RecordAction("Help_opened_" + keyword);
        }
        if (StateManager.Instance.CurrentStage == GameStage.FirstWorkshopOpen)
        {
            StateManager.Instance.SetState(GameStage.FirstHelpOpen);
            // StartCoroutine(DelayedStateUpdate());
        }
        helpPanel.RemoveFromClassList("help-hidden");
        helpPanel.style.display = DisplayStyle.Flex;

        var helpText = DataReader.Instance.GetHelpText(keyword);
        if (helpText == null)
        {
            helpTitle.text = "Help not found";
            helpDescription.text = "No help available for this topic.";
            helpImage.style.display =
            helpHighlights.style.display = DisplayStyle.None;
            return;
        }

        helpTitle.text = helpText.Title;
        helpDescription.text = string.Join("\n\n", helpText.Description);

        //  Visual (image)
        if (!string.IsNullOrEmpty(helpText.Visual))
        {
            var tex = Resources.Load<Texture2D>(helpText.Visual);
            if (tex != null)
            {
                helpImage.style.backgroundImage = new StyleBackground(tex);
                helpImage.style.display = DisplayStyle.Flex;
            }
            else
            {
                helpImage.style.display = DisplayStyle.None;
            }
        }
        else
        {
            helpImage.style.display = DisplayStyle.None;
        }

        // highlights
        if (helpText.Highlights?.Count > 0)
        {
            helpHighlights.text = "• " + string.Join("\n• ", helpText.Highlights);
            helpHighlights.style.display = DisplayStyle.Flex;
        }
        else helpHighlights.style.display = DisplayStyle.None;

    }

    void OnCloseButtonClicked()
    {
        helpPanel.AddToClassList("help-hidden");
        helpPanel.style.display = DisplayStyle.None;
    }
    IEnumerator DelayedStateUpdate()
    {
        yield return new WaitForSeconds(2f);
        StateManager.Instance.SetState(GameStage.FirstHelpOpen);

    }
}
