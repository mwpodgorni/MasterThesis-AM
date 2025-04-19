using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }

    public GameStage CurrentStage { get; private set; }

    private void Awake()
    {
        // Debug.Log("StateManager Awake called");
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

    public void MarkMiniGameSolved(int miniGameIndex)
    {
        // switch (miniGameIndex)
        // {
        //     case 1:
        //         CurrentStage = GameStage.MiniGame2;
        //         StageOneController.Instance.NetworkController().ResetNetwork();
        //         break;
        //     case 2:
        //         CurrentStage = GameStage.MiniGame3;
        //         StageOneController.Instance.NetworkController().EnableTraining();
        //         break;
        //     case 3:
        //         CurrentStage = GameStage.QLearning1;
        //         break;
        // }
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
                Debug.Log("SecondNetworkTrainedBad state reached");
                StageOneController.Instance.TutorialController().SetTutorialSteps(DataReader.Instance.SecondNetworkTrainedBad());
                StageOneController.Instance.TutorialController().StartTutorial();
                break;
            case GameStage.StageOneCompleted:
                StageOneController.Instance.TutorialController().SetTutorialSteps(DataReader.Instance.SecondNetworkTrainedGood());
                StageOneController.Instance.TutorialController().StartTutorial();
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
    QLearning1,
    Completed
}
