using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StageOneController : MonoBehaviour
{
    public static StageOneController Instance { get; private set; }
    public VisualElement ui;

    public VisualElement tutorialPanel;
    public VisualElement workshopPanel;
    public Button workshopOpenButton;
    public Button workshopCloseButton;
    public VisualElement helpPanel;

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
        tutorialPanel = ui.Q<VisualElement>("TutorialPanel");
        tutorialPanel.style.display = DisplayStyle.Flex;

        workshopPanel = ui.Q<VisualElement>("WorkshopPanel");
        workshopPanel.style.display = DisplayStyle.Flex;
        workshopPanel.AddToClassList("panel-up");
        helpPanel = ui.Q<VisualElement>("HelpPanel");
        helpPanel.style.display = DisplayStyle.Flex;

        tutorialPanel.AddToClassList("opacity-none");

        workshopOpenButton = ui.Q<Button>("WorkshopOpenButton");
        workshopOpenButton.clicked += OnWorkshopOpenButtonClicked;
        workshopCloseButton = ui.Q<Button>("WorkshopCloseButton");
        workshopCloseButton.clicked += OnWorkshopCloseButtonClicked;



        StartCoroutine(StartTutorial());
        // TODO: remove this debug code
        //     HideAllPanels();
        //     menu.style.display = DisplayStyle.Flex;
        //     menu.RemoveFromClassList("opacity-none");
        //     menu.pickingMode = PickingMode.Position;
    }

    IEnumerator StartTutorial()
    {
        yield return new WaitForSeconds(1f);
        tutorialPanel.RemoveFromClassList("opacity-none");
        TutorialController().StartTutorial();
    }
    public void OnWorkshopOpenButtonClicked()
    {
        workshopPanel.RemoveFromClassList("panel-up");
        workshopOpenButton.AddToClassList("opacity-none");
    }
    public void OnWorkshopCloseButtonClicked()
    {
        workshopPanel.AddToClassList("panel-up");
        workshopOpenButton.RemoveFromClassList("opacity-none");
    }
    public NetworkController NetworkController()
    {
        return GetComponent<NetworkController>();
    }
    public HelpController HelpController()
    {
        return GetComponent<HelpController>();
    }
    public TutorialController TutorialController()
    {
        // Debug.Log("TutorialController called");
        if (GetComponent<TutorialController>() == null)
        {
            Debug.LogError("TutorialController is null");
            return null;
        }
        return GetComponent<TutorialController>();
    }

}
