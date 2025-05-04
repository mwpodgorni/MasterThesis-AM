using UnityEngine;
public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }

    public GameStage CurrentStage { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentStage = GameStage.StartingPoint;
    }

    public void SetState(GameStage newState)
    {
        CurrentStage = newState;
        // Debug.Log($"Game state changed to: {CurrentStage}");
        UpdateBasedOnCurrentState();
    }
    public void UpdateBasedOnCurrentState()
    {
        switch (CurrentStage)
        {
            case GameStage.StartingPoint:
                MusicController.Instance.PlayNNMusic();
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
                StageOneController.Instance.ShowProgressBar();
                break;
            case GameStage.SecondNetworkTraining:
                StageOneController.Instance.NetworkController().DisableNetworkEditing();
                StageOneController.Instance.NetworkController().DisableTrainingButton();
                StageOneController.Instance.NetworkController().DisableTestButton();
                StageOneController.Instance.NetworkController().DisableLearningRateSlider();
                StageOneController.Instance.NetworkController().DisableTrainingCycleSlider();
                break;
            case GameStage.SecondNetworkTrained:
                StageOneController.Instance.ShowEvaluationOpenButton();
                StageOneController.Instance.TutorialController().SetTutorialSteps(DataReader.Instance.SecondNetworkTrained());
                StageOneController.Instance.TutorialController().StartTutorial();
                StageOneController.Instance.NetworkController().EnableNetworkEditing();
                StageOneController.Instance.NetworkController().EnableTrainingButton();
                StageOneController.Instance.NetworkController().EnableTestButton();
                StageOneController.Instance.NetworkController().EnableLearningRateSlider();
                StageOneController.Instance.NetworkController().EnableTrainingCycleSlider();
                StageOneController.Instance.HideOuterProgressBar();
                break;
            case GameStage.SecondNetworkTrainedBad:
                StageOneController.Instance.TutorialController().SetTutorialSteps(DataReader.Instance.SecondNetworkTrainedBad());
                StageOneController.Instance.TutorialController().StartTutorial();
                break;
            case GameStage.StageOneCompleted:
                StageOneController.Instance.TutorialController().SetTutorialSteps(DataReader.Instance.SecondNetworkTrainedGood());
                StageOneController.Instance.TutorialController().StartTutorial();
                StageOneController.Instance.ShowNextLevelButton();
                StageOneController.Instance.NetworkController().DisableTrainingButton();
                break;
            case GameStage.RLStartingPoint:
                MusicController.Instance.PlayRLMusic();
                break;
            case GameStage.RLOneStart:
                RLController.Instance.HideProgressBar();
                RLController.Instance.HideEvaluationOpenButton();
                RLController.Instance.HideWorkshopOpenButton();
                RLController.Instance.EnableStartButton();
                RLController.Instance.DisableStopButton();
                RLController.Instance.EnableRewardAdjusters();
                RLController.Instance.EnableTrainingSettings();
                break;
            case GameStage.RLOneStarted:
                RLController.Instance.DisableRewardAdjusters();
                RLController.Instance.DisableStartButton();
                RLController.Instance.EnableStopButton();
                RLController.Instance.ShowSpeedButtons();
                RLController.Instance.ShowProgressBar();
                RLController.Instance.DisableTrainingSettings();
                break;
            case GameStage.RLOneCompletedGood:
                RLController.Instance.HideOuterProgressBar();
                RLController.Instance.ShowWorkshopOpenButton();
                RLController.Instance.ShowNextLevelButton();
                RLController.Instance.DisableStopButton();
                RLController.Instance.EnableStartButton();
                RLController.Instance.ShowEvaluationOpenButton();
                break;
            case GameStage.RLOneCompletedBad:
                RLController.Instance.HideOuterProgressBar();
                RLController.Instance.DisableStopButton();
                RLController.Instance.ShowWorkshopOpenButton();
                RLController.Instance.EnableStartButton();
                RLController.Instance.ShowEvaluationOpenButton();
                break;
            case GameStage.RLTwoStart:
            case GameStage.RLThreeStart:
                RLController.Instance.HideProgressBar();
                RLController.Instance.HideEvaluationOpenButton();
                RLController.Instance.ShowSpeedButtons();
                RLController.Instance.DisableSpeedButtons();
                RLController.Instance.EnableStartButton();
                RLController.Instance.DisableStopButton();
                RLController.Instance.EnableRewardAdjusters();
                RLController.Instance.EnableTrainingSettings();
                break;
            case GameStage.RLTwoStarted:
            case GameStage.RLThreeStarted:
                RLController.Instance.DisableRewardAdjusters();
                RLController.Instance.DisableStartButton();
                RLController.Instance.EnableStopButton();
                RLController.Instance.ShowProgressBar();
                RLController.Instance.EnableSpeedButtons();
                RLController.Instance.DisableTrainingSettings();
                break;
            case GameStage.RLTwoCompletedBad:
            case GameStage.RLThreeCompletedBad:
                RLController.Instance.HideOuterProgressBar();
                RLController.Instance.ShowWorkshopOpenButton();
                RLController.Instance.DisableStopButton();
                RLController.Instance.EnableStartButton();
                break;
            case GameStage.RLTwoCompletedGood:
                RLController.Instance.ShowWorkshopOpenButton();
                RLController.Instance.HideOuterProgressBar();
                RLController.Instance.ShowNextLevelButton();
                break;
            case GameStage.RLThreeCompletedGood:
                RLController.Instance.HideOuterProgressBar();
                RLController.Instance.ShowNextLevelButton();
                ActivityTracker.Instance.StopTimer("StageTwoTime");
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
    SecondNetworkTraining,
    SecondNetworkTrained,
    SecondNetworkTrainedBad,
    StageOneCompleted,
    RLStartingPoint,
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
