using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class StageOneController : MonoBehaviour
{
    private string stageTwoSceneName = "StageTwo1";
    public static StageOneController Instance { get; private set; }
    public VisualElement ui;

    public VisualElement tutorialPanel;
    public VisualElement workshopPanel;
    public VisualElement evaluationPanel;
    public VisualElement evaluationStatus;
    public Button workshopOpenButton;
    public Button workshopCloseButton;
    public Button evaluationOpenButton;
    public Button evaluationCloseButton;
    public Button nextLevelButton;
    public VisualElement helpPanel;
    public VisualElement topbar;
    ProgressBar progressBar;
    ProgressBar outerProgressBar;
    public bool finishedTraining = false;
    bool evaluationOpen = false;
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
        evaluationStatus = ui.Q<VisualElement>("EvaluationStatus");
        HideEvaluationStatus();

        tutorialPanel.AddToClassList("opacity-none");
        topbar = ui.Q<VisualElement>("Topbar");

        workshopOpenButton = ui.Q<Button>("WorkshopOpenButton");
        workshopOpenButton.clicked += OnWorkshopOpenButtonClicked;
        workshopOpenButton.tooltip = "Open Workshop";
        workshopCloseButton = ui.Q<Button>("WorkshopCloseButton");
        workshopCloseButton.clicked += OnWorkshopCloseButtonClicked;
        evaluationOpenButton = ui.Q<Button>("EvaluationOpenButton");
        evaluationOpenButton.clicked += OnEvaluationOpenButtonClicked;
        evaluationCloseButton = ui.Q<Button>("EvaluationCloseButton");
        evaluationCloseButton.clicked += OnEvaluationCloseButtonClicked;
        nextLevelButton = ui.Q<Button>("NextLevelButton");
        nextLevelButton.clicked += LoadSecondStage;
        HideWorkshopOpenButton();
        HideEvaluationOpenButton();

        progressBar = ui.Q<ProgressBar>("ProgressBar");
        outerProgressBar = ui.Q<ProgressBar>("OuterProgressBar");
        HideProgressBar();
        HideOuterProgressBar();
        StartCoroutine(StartTutorial());
        if (ActivityTracker.Instance != null)
        {
            ActivityTracker.Instance.StartTimer("StageOneTime");
        }
        // TODO: remove this debug code
        //     HideAllPanels();
        //     menu.style.display = DisplayStyle.Flex;
        //     menu.RemoveFromClassList("opacity-none");
        //     menu.pickingMode = PickingMode.Position;
    }

    IEnumerator StartTutorial()
    {
        yield return new WaitForSeconds(1f);
        // Debug.Log("Starting tutorial");
        tutorialPanel.RemoveFromClassList("opacity-none");
        TutorialController().StartTutorial();
    }
    public void OnWorkshopOpenButtonClicked()
    {
        if (StateManager.Instance.CurrentStage == GameStage.StartingPoint)
        {
            StateManager.Instance.SetState(GameStage.FirstWorkshopOpen);
        }
        workshopPanel.RemoveFromClassList("panel-up");
        topbar.style.display = DisplayStyle.None;
        NetworkController().RedrawConnections();
    }
    public void OnWorkshopCloseButtonClicked()
    {
        if (StateManager.Instance.CurrentStage == GameStage.SecondNetworkTraining)
        {
            ShowOuterProgressBar();
        }
        NetworkController().ClearLines();
        workshopPanel.AddToClassList("panel-up");
        topbar.style.display = DisplayStyle.Flex;
    }
    public void OnEvaluationOpenButtonClicked()
    {
        evaluationOpen = true;
        CheckIfCompleted();
        evaluationPanel.RemoveFromClassList("panel-up");
        topbar.style.display = DisplayStyle.None;

    }
    public void OnEvaluationCloseButtonClicked()
    {
        evaluationOpen = false;
        evaluationPanel.AddToClassList("panel-up");
        topbar.style.display = DisplayStyle.Flex;
    }
    public void CheckIfCompleted()
    {
        if (StateManager.Instance.CurrentStage == GameStage.SecondNetworkTrained
            && finishedTraining)
        {
            int finishedCycles = StageOneController.Instance.EvaluationController().GetFinishedCycles();
            int correctPredictions = StageOneController.Instance.EvaluationController().GetCorrectPredictions();
            // Debug.Log($"Finished Cycles: {finishedCycles}, Correct Predictions: {correctPredictions}");
            if (finishedCycles / 2 > correctPredictions)
            {
                StateManager.Instance.SetState(GameStage.SecondNetworkTrainedBad);
            }
            else
            {
                StateManager.Instance.SetState(GameStage.StageOneCompleted);
            }
        }
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
            // Debug.LogError("TutorialController is null");
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
        if (ActivityTracker.Instance != null)
        {
            ActivityTracker.Instance.StopTimer("StageOneTime");
        }
        StateManager.Instance.SetState(GameStage.RLStartingPoint);
        StateManager.Instance.UpdateBasedOnCurrentState();
        SceneManager.LoadScene(stageTwoSceneName);
    }
    public void HideWorkshopOpenButton()
    {
        workshopOpenButton.AddToClassList("opacity-none");
        workshopOpenButton.style.display = DisplayStyle.None;
    }
    public void ShowWorkshopOpenButton()
    {
        workshopOpenButton.RemoveFromClassList("opacity-none");
        workshopOpenButton.style.display = DisplayStyle.Flex;
    }
    public void ShowEvaluationOpenButton()
    {
        evaluationOpenButton.RemoveFromClassList("opacity-none");
        evaluationOpenButton.style.display = DisplayStyle.Flex;
    }
    public void HideEvaluationOpenButton()
    {
        evaluationOpenButton.AddToClassList("opacity-none");
        evaluationOpenButton.style.display = DisplayStyle.None;
    }
    public void ShowProgressBar()
    {
        progressBar.style.display = DisplayStyle.Flex;
    }
    public void HideProgressBar()
    {
        progressBar.style.display = DisplayStyle.None;
    }
    public void ShowOuterProgressBar()
    {
        outerProgressBar.style.display = DisplayStyle.Flex;
    }
    public void HideOuterProgressBar()
    {
        outerProgressBar.style.display = DisplayStyle.None;
    }
    public void SetFinishedTraining(bool finished)
    {
        finishedTraining = finished;
    }
    public void ShowNextLevelButton()
    {
        nextLevelButton.style.display = DisplayStyle.Flex;
    }
    public void ChangeStateIfEvaluationOpen()
    {
        if (evaluationOpen)
        {
            CheckIfCompleted();
        }
    }
    public void HideEvaluationStatus()
    {
        evaluationStatus.style.display = DisplayStyle.None;
    }
    public void ShowEvaluationStatus()
    {
        Label statusValue = evaluationStatus.Q<Label>("StatusValue");

        if (StateManager.Instance.CurrentStage == GameStage.SecondNetworkTraining)
        {
            statusValue.text = "Evaluation in progress...";
            evaluationStatus.style.display = DisplayStyle.Flex;
        }
        if (EvaluationController() != null)
        {
            float loss = EvaluationController().GetFinalAverageLoss();
            float highErrorRatio = EvaluationController().GetErrorHigh() /
                                   (float)(EvaluationController().GetErrorLow() +
                                           EvaluationController().GetErrorMid() +
                                           EvaluationController().GetErrorHigh());
            int finishedCycles = EvaluationController().GetFinishedCycles();
            int correctPredictions = EvaluationController().GetCorrectPredictions();
            if (finishedCycles / 2 > correctPredictions)
            {
                statusValue.text = "Poor performance. Your network is not performing well.";
                statusValue.style.color = new StyleColor(new Color32(0x82, 0x3A, 0x30, 0xFF));

            }
            else
            {
                if (loss < 0.35f && highErrorRatio < 0.1f)
                {
                    statusValue.text = "Excellent! Your network is performing very well.";
                    statusValue.style.color = new StyleColor(new Color32(0x21, 0x63, 0x4F, 0xFF));
                }
                else
                {
                    statusValue.text = "Reasonable performance. Your network is functioning accurately.";
                    statusValue.style.color = new StyleColor(Color.yellow);
                }
            }
            evaluationStatus.style.display = DisplayStyle.Flex;
        }
    }
}
