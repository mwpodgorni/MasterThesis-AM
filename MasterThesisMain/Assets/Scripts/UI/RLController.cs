using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class RLController : MonoBehaviour
{
    Button _start;
    Button _speedNormal;
    Button _speed2x;
    Button _speed4x;
    Button _speed6x;
    Button _stop;
    Label _help;
    VisualElement _UI;
    VisualElement _rewardContainer;

    public VisualElement tutorialPanel;
    public VisualElement workshopPanel;
    public VisualElement evaluationPanel;
    public Button workshopOpenButton;
    public Button workshopCloseButton;
    public Button evaluationOpenButton;
    public Button evaluationCloseButton;
    public Button nextLevelButton;
    public VisualElement helpPanel;

    [SerializeField] VisualTreeAsset _rewardAdjusterTemplate;
    [SerializeField] RLManager _manager;

    [SerializeField] SceneAsset _nextScene;
    [SerializeField] List<Sprite> _tileSprites;

    [SerializeField] RLLevel _rlLevel = RLLevel.level1;

    Dictionary<TileType, Sprite> _tileSpritesDict = new();
    public static RLController Instance { get; private set; }
    ProgressBar _progressBar;

    string GetTutorialText
    {
        get
        {
            switch (_rlLevel)
            {
                case RLLevel.level1:
                    return "ReinforcementLearning";
                case RLLevel.level2:
                    return "RLPuzzle2";
                case RLLevel.level3:
                    return "RLPuzzle2";
            }

            return "";
        }
    }

    string GetSolvedText(bool solved)
    {
        switch (_rlLevel)
        {
            case RLLevel.level1:
                if (solved)
                {
                    return "RLPuzzle1Solved";
                }
                return "RLPuzzle1NotSolved";
            case RLLevel.level2:
                if (solved)
                {
                    return "RLPuzzle2Solved";
                }
                return "RLPuzzle2NotSolved";
            case RLLevel.level3:
                if (solved)
                {
                    return "RLPuzzle3Solved";
                }
                return "RLPuzzle3NotSolved";
        }

        return "";

    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {

        _tileSpritesDict.Add(TileType.Normal, _tileSprites[0]);
        _tileSpritesDict.Add(TileType.Wall, _tileSprites[1]);
        _tileSpritesDict.Add(TileType.Dangerous, _tileSprites[2]);
        _tileSpritesDict.Add(TileType.Collectible, _tileSprites[3]);
        _tileSpritesDict.Add(TileType.Goal, _tileSprites[4]);
        _tileSpritesDict.Add(TileType.Enemy, _tileSprites[5]);
        _tileSpritesDict.Add(TileType.Buff, _tileSprites[6]);

        _UI = GetComponent<UIDocument>().rootVisualElement;

        tutorialPanel = _UI.Q<VisualElement>("TutorialPanel");
        tutorialPanel.style.display = DisplayStyle.Flex;

        workshopPanel = _UI.Q<VisualElement>("WorkshopPanel");
        _help = _UI.Q<Label>("HelpRewardAdjuster");
        _rewardContainer = workshopPanel.Q<VisualElement>("RewardContainer");
        _start = workshopPanel.Q<Button>("Start");
        _stop = workshopPanel.Q<Button>("Stop");

        // speed buttons
        _speedNormal = workshopPanel.Q<Button>("Speed1X");
        _speed2x = workshopPanel.Q<Button>("Speed2X");
        _speed4x = workshopPanel.Q<Button>("Speed4X");
        _speed6x = workshopPanel.Q<Button>("Speed6X");
        _speedNormal.RegisterCallback<ClickEvent, GameSpeed>(SpeedUpHandler, GameSpeed.Normal);
        _speed2x.RegisterCallback<ClickEvent, GameSpeed>(SpeedUpHandler, GameSpeed.Fast);
        _speed4x.RegisterCallback<ClickEvent, GameSpeed>(SpeedUpHandler, GameSpeed.Faster);
        _speed6x.RegisterCallback<ClickEvent, GameSpeed>(SpeedUpHandler, GameSpeed.Fastest);


        _start.RegisterCallback<ClickEvent>(StartTrainingHandler);
        _stop.RegisterCallback<ClickEvent>(StopTrainingHandler);

        _help.RegisterCallback<ClickEvent>(evt => HelpController().ShowHelp("RewardAdjuster"));
        workshopPanel.style.display = DisplayStyle.Flex;
        workshopPanel.AddToClassList("panel-up");

        helpPanel = _UI.Q<VisualElement>("HelpPanel");
        helpPanel.style.display = DisplayStyle.Flex;

        evaluationPanel = _UI.Q<VisualElement>("EvaluationPanel");
        evaluationPanel.style.display = DisplayStyle.Flex;
        evaluationPanel.AddToClassList("panel-up");

        tutorialPanel.AddToClassList("opacity-none");

        workshopOpenButton = _UI.Q<Button>("WorkshopOpenButton");
        workshopOpenButton.clicked += OnWorkshopOpenButtonClicked;
        workshopCloseButton = workshopPanel.Q<Button>("WorkshopCloseButton");
        workshopCloseButton.clicked += OnWorkshopCloseButtonClicked;
        evaluationOpenButton = _UI.Q<Button>("EvaluationOpenButton");
        evaluationOpenButton.clicked += OnEvaluationOpenButtonClicked;
        evaluationCloseButton = _UI.Q<Button>("EvaluationCloseButton");
        evaluationCloseButton.clicked += OnEvaluationCloseButtonClicked;

        nextLevelButton = _UI.Q<Button>("NextLevelButton");
        nextLevelButton.clicked += LoadLevel;


        _progressBar = _UI.Q<ProgressBar>("ProgressBar");

        LoadRewardAdjusters();
        DisableStopButton();
        HideSpeedButtons();
        StartCoroutine(StartTutorial(GetTutorialText));
        if (_rlLevel == RLLevel.level1)
            StateManager.Instance.SetState(GameStage.RLOneStart);
        else if (_rlLevel == RLLevel.level2)
            StateManager.Instance.SetState(GameStage.RLTwoStart);
        else if (_rlLevel == RLLevel.level3)
            StateManager.Instance.SetState(GameStage.RLThreeStart);
    }

    IEnumerator StartTutorial(string name)
    {
        yield return new WaitForSeconds(1f);
        // Debug.Log("Starting tutorial");
        tutorialPanel.RemoveFromClassList("opacity-none");
        var steps = DataReader.Instance.GetTutorialSteps(name);
        TutorialController().SetTutorialSteps(steps);
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
    public void OnEvaluationOpenButtonClicked()
    {
        var good = new[]
            {
                GameStage.RLOneCompletedGood,
                GameStage.RLTwoCompletedGood,
                GameStage.RLThreeCompletedGood
            };
        var bad = new[]
            {
                GameStage.RLOneCompletedBad,
                GameStage.RLTwoCompletedBad,
                GameStage.RLThreeCompletedBad
            };
        if (good.Contains(StateManager.Instance.CurrentStage))
        {
            StartCoroutine(StartTutorial(GetSolvedText(true)));
            // TutorialController().AddToEvent(LoadLevel);
        }
        else if (bad.Contains(StateManager.Instance.CurrentStage))
        {
            StartCoroutine(StartTutorial(GetSolvedText(false)));
        }
        EvaluationController().UpdateEvaluationData(_manager.currentEval);
        evaluationPanel.RemoveFromClassList("panel-up");
        evaluationOpenButton.AddToClassList("opacity-none");
    }
    public void OnEvaluationCloseButtonClicked()
    {
        evaluationPanel.AddToClassList("panel-up");
        evaluationOpenButton.RemoveFromClassList("opacity-none");
    }
    public void HideProgressBar()
    {
        _progressBar.AddToClassList("opacity-none");
    }
    public void ShowProgressBar()
    {
        _progressBar.RemoveFromClassList("opacity-none");
    }
    public void OnLevelFinished(bool solved)
    {
        if (solved)
        {
            if (_rlLevel == RLLevel.level1)
            {
                StateManager.Instance.SetState(GameStage.RLOneCompletedGood);
            }
            else if (_rlLevel == RLLevel.level2)
            {
                StateManager.Instance.SetState(GameStage.RLTwoCompletedGood);
            }
            else if (_rlLevel == RLLevel.level3)
            {
                StateManager.Instance.SetState(GameStage.RLThreeCompletedGood);
            }
        }
        else
        {
            if (_rlLevel == RLLevel.level1)
            {
                StateManager.Instance.SetState(GameStage.RLOneCompletedBad);
            }
            else if (_rlLevel == RLLevel.level2)
            {
                StateManager.Instance.SetState(GameStage.RLTwoCompletedBad);
            }
            else if (_rlLevel == RLLevel.level3)
            {
                StateManager.Instance.SetState(GameStage.RLThreeCompletedBad);
            }
        }
        // StartT
        // RLPuzzle1Completed
        // StartCoroutine(StartTutorial(GetSolvedText(solved)));
        StartCoroutine(StartTutorial("RLPuzzleCompleted"));


    }

    void LoadRewardAdjusters()
    {
        var tiles = _manager.GetObservedTiles();

        foreach (var tile in tiles)
        {
            VisualElement rewardAdjuster = _rewardAdjusterTemplate.CloneTree();

            var slider = rewardAdjuster.Q<Slider>();
            var image = rewardAdjuster.Q<VisualElement>("Image");
            var label = rewardAdjuster.Q<Label>();

            image.style.backgroundImage = new StyleBackground(_tileSpritesDict[tile]);

            slider.RegisterCallback<ChangeEvent<float>>(evt => SetTileRewardHandler(evt, tile, label, slider));

            _rewardContainer.Add(rewardAdjuster);
        }
    }

    void SetTileRewardHandler(ChangeEvent<float> evt, TileType tile, Label label, Slider slider)
    {
        var value = evt.newValue;

        // Snap to 0, 0.5, 1
        value = SnapToValue(value, 0, 0.05f);

        value = SnapToValue(value, 0.5f, 0.05f);
        value = SnapToValue(value, 1f, 0.05f);

        value = SnapToValue(value, -0.5f, 0.05f);
        value = SnapToValue(value, -1f, 0.05f);

        _manager.SetReward(tile, value);
        slider.value = value;
        label.text = value.ToString();
    }

    void StartTrainingHandler(ClickEvent evt)
    {
        if (StateManager.Instance.CurrentStage == GameStage.RLOneStart)
        {
            StateManager.Instance.SetState(GameStage.RLOneStarted);
        }
        else if (StateManager.Instance.CurrentStage == GameStage.RLTwoStart)
        {
            StateManager.Instance.SetState(GameStage.RLTwoStarted);
        }
        else if (StateManager.Instance.CurrentStage == GameStage.RLThreeStart)
        {
            StateManager.Instance.SetState(GameStage.RLThreeStarted);
        }
        _manager.StartTraining();
    }

    void StopTrainingHandler(ClickEvent evt)
    {
        EnableRewardAdjusters();
        EnableStartButton();
        DisableStopButton();
        DisableSpeedButtons();
        _manager.StopTraining();
    }

    void SpeedUpHandler(ClickEvent evt, GameSpeed s)
    {
        _manager.SetSpeed(s);
    }

    public void EnableRewardAdjusters()
    {
        var rewardAdjusters = _rewardContainer.Query("RewardAdjuster").ToList();

        foreach (var adjuster in rewardAdjusters)
        {
            adjuster.SetEnabled(true);
        }
    }

    public void DisableRewardAdjusters()
    {
        var rewardAdjusters = _rewardContainer.Query("RewardAdjuster").ToList();

        foreach (var adjuster in rewardAdjusters)
        {
            adjuster.SetEnabled(false);
        }
    }

    public void EnableStartButton()
    {
        _start.SetEnabled(true);
    }

    public void DisableStartButton()
    {
        _start.SetEnabled(false);
    }

    public void EnableStopButton()
    {
        _stop.SetEnabled(true);
    }

    public void DisableStopButton()
    {
        _stop.SetEnabled(false);
    }

    public void ShowSpeedButtons()
    {
        _speedNormal.RemoveFromClassList("opacity-none");
        _speed2x.RemoveFromClassList("opacity-none");
        _speed4x.RemoveFromClassList("opacity-none");
        _speed6x.RemoveFromClassList("opacity-none");
    }
    public void HideSpeedButtons()
    {
        _speedNormal.AddToClassList("opacity-none");
        _speed2x.AddToClassList("opacity-none");
        _speed4x.AddToClassList("opacity-none");
        _speed6x.AddToClassList("opacity-none");
    }
    public void EnableSpeedButtons()
    {
        _speedNormal.SetEnabled(true);
        _speed2x.SetEnabled(true);
        _speed4x.SetEnabled(true);
        _speed6x.SetEnabled(true);
    }
    public void DisableSpeedButtons()
    {
        _speedNormal.SetEnabled(false);
        _speed2x.SetEnabled(false);
        _speed4x.SetEnabled(false);
        _speed6x.SetEnabled(false);
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
    public RLEvaluationController EvaluationController()
    {
        return GetComponent<RLEvaluationController>();
    }
    public void ShowWorkshopOpenButton()
    {
        workshopOpenButton.RemoveFromClassList("opacity-none");
    }
    public void ShowEvaluationOpenButton()
    {
        evaluationOpenButton.RemoveFromClassList("opacity-none");
    }
    public void HideWorkshopOpenButton()
    {
        workshopOpenButton.AddToClassList("opacity-none");
    }
    public void HideEvaluationOpenButton()
    {
        evaluationOpenButton.AddToClassList("opacity-none");
    }
    public void LoadLevel()
    {
        SceneManager.LoadScene(_nextScene.name);
    }
    public void ShowNextLevelButton()
    {
        nextLevelButton.style.display = DisplayStyle.Flex;
    }
    public enum RLLevel
    {
        level1,
        level2,
        level3
    }

    float SnapToValue(float input, float snapToValue, float sensitivity)
    {

        if (input <= (snapToValue + sensitivity) && snapToValue <= input) return snapToValue;
        if (input >= (snapToValue - sensitivity) && snapToValue >= input) return snapToValue;

        return input;
    }
}
