using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor;

public class StageOneController : MonoBehaviour
{
    [SerializeField] private SceneAsset stageTwoScene;
    public static StageOneController Instance { get; private set; }
    public VisualElement ui;

    public VisualElement tutorialPanel;
    public VisualElement workshopPanel;
    public VisualElement evaluationPanel;
    public Button workshopOpenButton;
    public Button workshopCloseButton;
    public Button evaluationOpenButton;
    public Button evaluationCloseButton;
    public VisualElement helpPanel;

    public bool firstEvaluationView = true;
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

        evaluationPanel = ui.Q<VisualElement>("EvaluationPanel");
        evaluationPanel.style.display = DisplayStyle.Flex;
        evaluationPanel.AddToClassList("panel-up");

        tutorialPanel.AddToClassList("opacity-none");

        workshopOpenButton = ui.Q<Button>("WorkshopOpenButton");
        workshopOpenButton.clicked += OnWorkshopOpenButtonClicked;
        workshopOpenButton.tooltip = "Open Workshop";
        workshopCloseButton = ui.Q<Button>("WorkshopCloseButton");
        workshopCloseButton.clicked += OnWorkshopCloseButtonClicked;
        evaluationOpenButton = ui.Q<Button>("EvaluationOpenButton");
        evaluationOpenButton.clicked += OnEvaluationOpenButtonClicked;
        evaluationCloseButton = ui.Q<Button>("EvaluationCloseButton");
        evaluationCloseButton.clicked += OnEvaluationCloseButtonClicked;


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
        Debug.Log("Starting tutorial");
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
        NetworkController().ClearLines();
        workshopPanel.AddToClassList("panel-up");
        workshopOpenButton.RemoveFromClassList("opacity-none");
    }
    public void OnEvaluationOpenButtonClicked()
    {
        if (firstEvaluationView)
        {
            firstEvaluationView = false;
            TutorialController().ShowNextButton();
            TutorialController().SetTypeText(true);
            TutorialController().SetTutorialSteps(DataReader.Instance.ThirdPuzzleSolved());
            TutorialController().StartTutorial();
        }

        evaluationPanel.RemoveFromClassList("panel-up");
        evaluationOpenButton.AddToClassList("opacity-none");
    }
    public void OnEvaluationCloseButtonClicked()
    {
        evaluationPanel.AddToClassList("panel-up");
        evaluationOpenButton.RemoveFromClassList("opacity-none");
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
    public EvaluationController EvaluationController()
    {
        return GetComponent<EvaluationController>();
    }
    public void LoadSecondStage()
    {
        SceneManager.LoadScene(stageTwoScene.name);
    }
}
