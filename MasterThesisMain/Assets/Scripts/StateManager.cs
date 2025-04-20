using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }

    public GameStage CurrentStage { get; private set; }

    private void Awake()
    {
        Debug.Log("StateManager Awake called");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // Debug.Log("StateManager Awake called2");
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentStage = GameStage.StartingPoint;
    }

    public void SetState(GameStage newState)
    {
        CurrentStage = newState;
        Debug.Log($"Game state changed to: {CurrentStage}");
        UpdateBasedOnCurrentState();
    }
    public void UpdateBasedOnCurrentState()
    {
        switch (CurrentStage)
        {
            case GameStage.StartingPoint:
                break;
            case GameStage.FirstWorkshopOpen:
                StageOneController.Instance.TutorialController().HideNextButton();
                StageOneController.Instance.NetworkController().DisableInputLayerButtons();
                StageOneController.Instance.NetworkController().DisableHiddenLayerButtons();
                StageOneController.Instance.NetworkController().DisableOutputLayerButtons();
                StageOneController.Instance.TutorialController().OnNextButtonClicked();
                break;
            case GameStage.FirstHelpOpen:
                StageOneController.Instance.TutorialController().ShowNextButton();
                StageOneController.Instance.NetworkController().EnableInputLayerButtons();
                StageOneController.Instance.NetworkController().EnableHiddenLayerButtons();
                StageOneController.Instance.NetworkController().EnableOutputLayerButtons();
                StageOneController.Instance.NetworkController().ShowNetworkActionPanel();
                StageOneController.Instance.NetworkController().HideTrainingCycleForm();
                StageOneController.Instance.NetworkController().HideLearningRateForm();
                StageOneController.Instance.NetworkController().HideTrainButton();
                StageOneController.Instance.TutorialController().OnNextButtonClicked();
                break;
            case GameStage.FirstNetworkValidated:
                StageOneController.Instance.TutorialController().SetTypeText(true);
                StageOneController.Instance.TutorialController().ShowNextButton();
                StageOneController.Instance.TutorialController().SetTutorialSteps(DataReader.Instance.FirstNetworkValid());
                StageOneController.Instance.TutorialController().StartTutorial();
                StageOneController.Instance.NetworkController().ResetNetwork();
                break;
            case GameStage.SecondNetworkValidated:
                StageOneController.Instance.TutorialController().SetTypeText(true);
                StageOneController.Instance.TutorialController().ShowNextButton();
                StageOneController.Instance.TutorialController().SetTutorialSteps(DataReader.Instance.SecondNetworkValid());
                StageOneController.Instance.TutorialController().StartTutorial();
                StageOneController.Instance.NetworkController().ShowTrainingCycleForm();
                StageOneController.Instance.NetworkController().ShowLearningRateForm();
                StageOneController.Instance.NetworkController().ShowTrainButton();
                break;
            case GameStage.SecondNetworkTrained:
                StageOneController.Instance.ShowEvaluationOpenButton();
                break;
            case GameStage.SecondNetworkTrainedBad:
                StageOneController.Instance.TutorialController().SetTutorialSteps(DataReader.Instance.SecondNetworkTrainedBad());
                StageOneController.Instance.TutorialController().StartTutorial();
                break;
            case GameStage.StageOneCompleted:
                StageOneController.Instance.TutorialController().SetTutorialSteps(DataReader.Instance.SecondNetworkTrainedGood());
                StageOneController.Instance.TutorialController().StartTutorial();
                break;
            case GameStage.RLOneStart:
                RLController.Instance.HideProgressBar();
                RLController.Instance.HideEvaluationOpenButton();
                RLController.Instance.HideWorkshopOpenButton();
                break;
            case GameStage.RLOneStarted:
                RLController.Instance.DisableRewardAdjusters();
                RLController.Instance.DisableStartButton();
                RLController.Instance.EnableStopButton();
                RLController.Instance.ShowSpeedButtons();
                RLController.Instance.ShowProgressBar();
                break;
            case GameStage.RLOneCompletedGood:
                RLController.Instance.ShowNextLevelButton();
                RLController.Instance.DisableStopButton();
                RLController.Instance.EnableStartButton();
                RLController.Instance.ShowEvaluationOpenButton();
                break;
            case GameStage.RLOneCompletedBad:
                RLController.Instance.DisableStopButton();
                RLController.Instance.EnableStartButton();
                RLController.Instance.ShowEvaluationOpenButton();
                break;
            case GameStage.RLTwoStart:
            case GameStage.RLThreeStart:
                RLController.Instance.HideProgressBar();
                RLController.Instance.HideEvaluationOpenButton();
                break;
            case GameStage.RLTwoStarted:
            case GameStage.RLThreeStarted:
                RLController.Instance.ShowEvaluationOpenButton();
                RLController.Instance.DisableRewardAdjusters();
                RLController.Instance.DisableStartButton();
                RLController.Instance.EnableStopButton();
                RLController.Instance.EnableSpeedButtons();
                RLController.Instance.ShowProgressBar();
                break;
            case GameStage.RLTwoCompletedBad:
                break;
            case GameStage.RLTwoCompletedGood:
                RLController.Instance.ShowNextLevelButton();
                break;
            case GameStage.RLThreeCompletedGood:
                RLController.Instance.ShowNextLevelButton();
                break;
            case GameStage.RLThreeCompletedBad:
                break;

        }
    }
}

public enum GameStage
{
    StartingPoint,
    FirstWorkshopOpen,
    FirstHelpOpen,
    FirstNetworkValidated,
    SecondNetworkValidated,
    SecondNetworkTrained,
    SecondNetworkTrainedBad,
    StageOneCompleted,
    RLOneStart,
    RLOneStarted,
    RLOneCompletedGood,
    RLOneCompletedBad,
    RLTwoStart,
    RLTwoStarted,
    RLTwoCompletedBad,
    RLTwoCompletedGood,
    RLThreeStart,
    RLThreeStarted,
    RLThreeCompletedBad,
    RLThreeCompletedGood,
}
