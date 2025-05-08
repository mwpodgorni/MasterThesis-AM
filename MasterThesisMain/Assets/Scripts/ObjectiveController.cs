using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class ObjectiveController : MonoBehaviour
{
    public VisualElement ui;
    public VisualElement objectivePanel;
    public Label objectiveText;


    public void OnEnable()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;

        objectivePanel = ui.Q<VisualElement>("ObjectivePanel");
        objectiveText = ui.Q<Label>("ObjectiveText");
        HideObjective();
    }
    public void SetObjective(string objective)
    {
        objectiveText.text = objective;
        objectivePanel.style.display = DisplayStyle.Flex;
    }
    public void HideObjective()
    {
        objectivePanel.style.display = DisplayStyle.None;
    }
    public void ShowObjective()
    {
        objectivePanel.style.display = DisplayStyle.Flex;
    }

}
