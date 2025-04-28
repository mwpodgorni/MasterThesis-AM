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

        // start hidden
        helpPanel.style.display = DisplayStyle.None;
        helpPanel.AddToClassList("help-hidden");
    }

    public void ShowHelp(string keyword)
    {
        Debug.Log("HELP SHOW HELP CALLED" + keyword);
        ActivityTracker.Instance.RecordAction("Help_opened_" + keyword);
        if (StateManager.Instance.CurrentStage == GameStage.FirstWorkshopOpen)
        {
            StateManager.Instance.SetState(GameStage.FirstHelpOpen);
            // StartCoroutine(DelayedStateUpdate());
        }
        // un-hide
        helpPanel.RemoveFromClassList("help-hidden");
        helpPanel.style.display = DisplayStyle.Flex;

        Debug.Log("Showing help for: " + keyword);
        var helpText = DataReader.Instance.GetHelpText(keyword);
        Debug.Log("Help text: " + helpText);
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

        // — Visual (image) —
        if (!string.IsNullOrEmpty(helpText.Visual))
        {
            Debug.Log($"[Help] Loading image at Resources/{helpText.Visual}");
            var tex = Resources.Load<Texture2D>(helpText.Visual);
            if (tex != null)
            {
                Debug.Log($"[Help] Successfully loaded texture: {tex.name}");
                helpImage.style.backgroundImage = new StyleBackground(tex);
                helpImage.style.display = DisplayStyle.Flex;
            }
            else
            {
                Debug.Log($"[Help] Failed to load texture at path: {helpText.Visual}");
                helpImage.style.display = DisplayStyle.None;
            }
        }
        else
        {
            Debug.LogWarning("[Help] No visual path provided.");
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
