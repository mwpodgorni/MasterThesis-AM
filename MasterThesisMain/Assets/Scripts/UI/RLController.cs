using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    Label _helpLearningConfiguration;
    VisualElement _UI;
    VisualElement _rewardContainer;

    public VisualElement tutorialPanel;
    public VisualElement workshopPanel;
    public VisualElement rewardAdjustments;
    public VisualElement settingsAdjustments;
    public VisualElement evaluationPanel;
    public VisualElement evaluationStatus;

    public Button workshopOpenButton;
    public Button workshopCloseButton;
    public Button evaluationOpenButton;
    public Button evaluationCloseButton;
    public Button nextLevelButton;
    public Button surveyButton;
    public VisualElement helpPanel;

    public VisualElement learningRate;
    public VisualElement decayRate;
    public VisualElement maxSteps;
    public VisualElement maxEpisodes;

    [SerializeField] VisualTreeAsset _rewardAdjusterTemplate;
    [SerializeField] RLManager _manager;
    [SerializeField] InputReader _input;

    [SerializeField] string _nextScene;
    [SerializeField] List<Sprite> _tileSprites;

    [SerializeField] RLLevel _rlLevel = RLLevel.level1;

    Dictionary<TileType, Sprite> _tileSpritesDict = new();
    public static RLController Instance { get; private set; }
    ProgressBar _progressBar;
    ProgressBar _outerProgressBar;

    bool evaluationOpen = false;

    bool _workShopFirstTime = true;
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
                    return "RLPuzzle3";
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
        string levelName = GetLevelName();

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

        // Workshop Panel
        workshopPanel = _UI.Q<VisualElement>("WorkshopPanel");
        rewardAdjustments = _UI.Q<VisualElement>("Reward");
        settingsAdjustments = _UI.Q<VisualElement>("Settings");

        // Reward Adjustments
        _help = _UI.Q<Label>("HelpRewardAdjuster");
        _helpLearningConfiguration = _UI.Q<Label>("HelpLearningConfiguration");
        _rewardContainer = _UI.Q<VisualElement>("RewardContainer");
        _start = rewardAdjustments.Q<Button>("Start");
        _stop = rewardAdjustments.Q<Button>("Stop");

        _start.RegisterCallback<ClickEvent>(StartTrainingHandler);
        _stop.RegisterCallback<ClickEvent>(StopTrainingHandler);
        if (ActivityTracker.Instance != null)
        {
            _start.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "Start_Pressed"); });
            _stop.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "Stop_Pressed"); });
        }
        // speed buttons
        #region Speed Buttons
        _speedNormal = rewardAdjustments.Q<Button>("Speed1X");
        _speed2x = rewardAdjustments.Q<Button>("Speed2X");
        _speed4x = rewardAdjustments.Q<Button>("Speed4X");
        _speed6x = rewardAdjustments.Q<Button>("Speed6X");
        _speedNormal.RegisterCallback<ClickEvent, GameSpeed>(SpeedUpHandler, GameSpeed.Normal);
        _speed2x.RegisterCallback<ClickEvent, GameSpeed>(SpeedUpHandler, GameSpeed.Fast);
        _speed4x.RegisterCallback<ClickEvent, GameSpeed>(SpeedUpHandler, GameSpeed.Faster);
        _speed6x.RegisterCallback<ClickEvent, GameSpeed>(SpeedUpHandler, GameSpeed.Fastest);
        #endregion

        // Settings Adjustements
        #region Training Settings
        learningRate = settingsAdjustments.Q<VisualElement>("LearningRate");
        decayRate = settingsAdjustments.Q<VisualElement>("DecayRate");
        maxSteps = settingsAdjustments.Q<VisualElement>("MaxSteps");
        maxEpisodes = settingsAdjustments.Q<VisualElement>("MaxEpisodes");

        learningRate.Q<Slider>().RegisterCallback<ChangeEvent<float>>(SetLearningRateHandler);
        decayRate.Q<Slider>().RegisterCallback<ChangeEvent<float>>(SetDecayRateHandler);
        maxSteps.Q<Slider>().RegisterCallback<ChangeEvent<float>>(SetMaxStepsHandler);
        maxEpisodes.Q<Slider>().RegisterCallback<ChangeEvent<float>>(SetMaxEpisodesHandler);

        learningRate.style.display = DisplayStyle.None;
        decayRate.style.display = DisplayStyle.None;
        maxSteps.style.display = DisplayStyle.None;
        maxEpisodes.style.display = DisplayStyle.None;
        if (ActivityTracker.Instance != null)
        {
            learningRate.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "RL_LearningRate_Interacted"); });
            decayRate.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "RL_ExplorationRate_Interacted"); });
            maxSteps.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "RL_MaxSteps_Interacted"); });
            maxEpisodes.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "RL_MaxEpisodes_Interacted"); });
        }
        #endregion

        _help.RegisterCallback<ClickEvent>(evt => HelpController().ShowHelp("RewardAdjuster"));
        _helpLearningConfiguration.RegisterCallback<ClickEvent>(evt => HelpController().ShowHelp("RLConfiguration"));
        if (ActivityTracker.Instance != null)
        {
            _help.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "Help_Reward_Pressed"); });
            _helpLearningConfiguration.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + "Help_Settings_Pressed"); });
        }
        workshopPanel.style.display = DisplayStyle.Flex;
        workshopPanel.AddToClassList("panel-up");

        helpPanel = _UI.Q<VisualElement>("HelpPanel");
        helpPanel.style.display = DisplayStyle.Flex;

        evaluationPanel = _UI.Q<VisualElement>("EvaluationPanel");
        evaluationPanel.style.display = DisplayStyle.Flex;
        evaluationPanel.AddToClassList("panel-up");
        evaluationStatus = _UI.Q<VisualElement>("EvaluationStatus");
        HideEvaluationStatus();

        tutorialPanel.AddToClassList("opacity-none");

        workshopOpenButton = _UI.Q<Button>("WorkshopOpenButton");
        workshopOpenButton.clicked += OnWorkshopOpenButtonClicked;
        workshopCloseButton = workshopPanel.Q<Button>("WorkshopCloseButton");
        workshopCloseButton.clicked += OnWorkshopCloseButtonClicked;
        evaluationOpenButton = _UI.Q<Button>("EvaluationOpenButton");
        evaluationOpenButton.clicked += OnEvaluationOpenButtonClicked;
        evaluationCloseButton = _UI.Q<Button>("EvaluationCloseButton");
        evaluationCloseButton.clicked += OnEvaluationCloseButtonClicked;
        surveyButton = _UI.Q<Button>("SurveyButton");
        surveyButton.clicked += OnSurveyButtonClicked;

        nextLevelButton = _UI.Q<Button>("NextLevelButton");
        nextLevelButton.clicked += LoadLevel;

        _progressBar = _UI.Q<ProgressBar>("ProgressBar");
        _outerProgressBar = _UI.Q<ProgressBar>("OuterProgressBar");
        HideOuterProgressBar();
        HideEvaluationOpenButton();
        foreach (var setting in _manager.GetRLSettings())
        {
            if (setting == RLSettings.LearningRate)
            {
                learningRate.style.display = DisplayStyle.Flex;
                continue;
            }
            if (setting == RLSettings.DecayRate)
            {
                decayRate.style.display = DisplayStyle.Flex;
                continue;
            }
            if (setting == RLSettings.Steps)
            {
                maxSteps.style.display = DisplayStyle.Flex;
                continue;
            }
            if (setting == RLSettings.Episodes)
            {
                maxEpisodes.style.display = DisplayStyle.Flex;
                continue;
            }
        }

        LoadRewardAdjusters();
        DisableStopButton();
        HideSpeedButtons();
        StartCoroutine(StartTutorial(GetTutorialText));
        if (_rlLevel == RLLevel.level1)
        {
            if (ActivityTracker.Instance != null)
            {
                ActivityTracker.Instance.StartTimer("StageTwoTime");
            }
            StateManager.Instance.SetState(GameStage.RLOneStart);
        }
        else if (_rlLevel == RLLevel.level2)
        {
            StateManager.Instance.SetState(GameStage.RLTwoStart);
        }
        else if (_rlLevel == RLLevel.level3)
        {
            StateManager.Instance.SetState(GameStage.RLThreeStart);
        }

        SetUpSettingsImages();
        if (_rlLevel != RLLevel.level1) _workShopFirstTime = false;

    }

    IEnumerator StartTutorial(string name)
    {
        yield return new WaitForSeconds(1f);
        tutorialPanel.RemoveFromClassList("opacity-none");
        var steps = DataReader.Instance.GetTutorialSteps(name);
        TutorialController().SetTutorialSteps(steps);
        TutorialController().StartTutorial();
    }

    public void OnWorkshopOpenButtonClicked()
    {
        HideOuterProgressBar();
        if (_workShopFirstTime)
        {
            _workShopFirstTime = false;
            StartCoroutine(StartTutorial("RLWorkshop"));
        }

        workshopPanel.RemoveFromClassList("panel-up");
        workshopOpenButton.AddToClassList("opacity-none");

        _input.DisableCameraActions();
    }
    public void OnWorkshopCloseButtonClicked()
    {
        if (StateManager.Instance.CurrentStage == GameStage.RLOneStarted ||
            StateManager.Instance.CurrentStage == GameStage.RLTwoStarted ||
            StateManager.Instance.CurrentStage == GameStage.RLThreeStarted)
        {
            ShowOuterProgressBar();
        }

        workshopPanel.AddToClassList("panel-up");
        workshopOpenButton.RemoveFromClassList("opacity-none");

        _input.EnableCameraActions();
    }

    public void RevertState()
    {
        if (StateManager.Instance.CurrentStage == GameStage.RLOneCompletedBad
        || StateManager.Instance.CurrentStage == GameStage.RLTwoCompletedGood)
        {
            StateManager.Instance.SetState(GameStage.RLOneStart);
        }
        else if (StateManager.Instance.CurrentStage == GameStage.RLTwoCompletedBad
        || StateManager.Instance.CurrentStage == GameStage.RLTwoCompletedGood)
        {
            StateManager.Instance.SetState(GameStage.RLTwoStart);
        }
        else if (StateManager.Instance.CurrentStage == GameStage.RLThreeCompletedBad
        || StateManager.Instance.CurrentStage == GameStage.RLThreeCompletedGood)
        {
            StateManager.Instance.SetState(GameStage.RLThreeStart);
        }
    }
    public void OnEvaluationOpenButtonClicked()
    {
        evaluationOpen = true;
        CheckCompletion();

        UpdateEvaluation();
        evaluationPanel.RemoveFromClassList("panel-up");
        evaluationOpenButton.AddToClassList("opacity-none");

        RevertState();
        _input.DisableCameraActions();
    }
    public void CheckCompletion()
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
    }
    public void OnEvaluationCloseButtonClicked()
    {
        evaluationOpen = false;
        evaluationPanel.AddToClassList("panel-up");
        evaluationOpenButton.RemoveFromClassList("opacity-none");
        ShowEvaluationOpenButton();

        _input.EnableCameraActions();
    }
    public void HideProgressBar()
    {
        _progressBar.AddToClassList("opacity-none");
    }
    public void ShowProgressBar()
    {
        _progressBar.RemoveFromClassList("opacity-none");
    }
    public void ShowOuterProgressBar()
    {
        _outerProgressBar.style.display = DisplayStyle.Flex;
    }
    public void HideOuterProgressBar()
    {
        _outerProgressBar.style.display = DisplayStyle.None;
    }
    public void UpdateEvaluation()
    {
        EvaluationController().UpdateEvaluationData(_manager.currentEval);
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
        if (evaluationOpen)
        {
            CheckCompletion();

        }
        else
        {
            StartCoroutine(StartTutorial("RLPuzzleCompleted"));

        }

    }

    void LoadRewardAdjusters()
    {
        var tiles = _manager.GetObservedTiles();

        foreach (var tile in tiles)
        {
            string levelName = GetLevelName();
            VisualElement rewardAdjuster = _rewardAdjusterTemplate.CloneTree();

            var slider = rewardAdjuster.Q<Slider>();
            var image = rewardAdjuster.Q<VisualElement>("Image");
            var label = rewardAdjuster.Q<Label>();

            image.style.backgroundImage = new StyleBackground(_tileSpritesDict[tile]);

            slider.RegisterCallback<ChangeEvent<float>>(evt => SetTileRewardHandler(evt, tile, label, slider));
            if (ActivityTracker.Instance != null)
            {
                slider.RegisterCallback<ClickEvent>(evt => { ActivityTracker.Instance.RecordAction(levelName + tile.ToString()); });
            }
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
        _manager.StopTraining();
        if (StateManager.Instance.CurrentStage == GameStage.RLOneStarted)
        {
            StateManager.Instance.SetState(GameStage.RLOneStart);
        }
        else if (StateManager.Instance.CurrentStage == GameStage.RLTwoStarted)
        {
            StateManager.Instance.SetState(GameStage.RLTwoStart);
        }
        else if (StateManager.Instance.CurrentStage == GameStage.RLThreeStarted)
        {
            StateManager.Instance.SetState(GameStage.RLThreeStart);
        }
    }

    void SpeedUpHandler(ClickEvent evt, GameSpeed s)
    {
        _manager.SetSpeed(s);
    }

    public void ShowSpeedButtons()
    {
        _speedNormal.style.display = DisplayStyle.Flex;
        _speed2x.style.display = DisplayStyle.Flex;
        _speed4x.style.display = DisplayStyle.Flex;
        _speed6x.style.display = DisplayStyle.Flex;
    }
    public void HideSpeedButtons()
    {
        _speedNormal.style.display = DisplayStyle.None;
        _speed2x.style.display = DisplayStyle.None;
        _speed4x.style.display = DisplayStyle.None;
        _speed6x.style.display = DisplayStyle.None;
    }

    void SetDecayRateHandler(ChangeEvent<float> evt)
    {
        _manager.epsilonDecayRate = evt.newValue;
    }

    void SetMaxStepsHandler(ChangeEvent<float> evt)
    {
        _manager.maxStepPerEpoch = (int)evt.newValue;
    }

    void SetMaxEpisodesHandler(ChangeEvent<float> evt)
    {
        _manager.maxEpisodes = (int)evt.newValue;
    }

    void SetLearningRateHandler(ChangeEvent<float> evt)
    {
        _manager.learningRate = evt.newValue;
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
    public RLEvaluationController EvaluationController()
    {
        return GetComponent<RLEvaluationController>();
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
    public void HideWorkshopOpenButton()
    {
        workshopOpenButton.AddToClassList("opacity-none");
        workshopOpenButton.style.display = DisplayStyle.None;
    }
    public void HideEvaluationOpenButton()
    {
        evaluationOpenButton.AddToClassList("opacity-none");
        evaluationOpenButton.style.display = DisplayStyle.None;
    }
    public void LoadLevel()
    {
        SceneManager.LoadScene(_nextScene);
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
    public void SetUpSettingsImages()
    {
        var learningRateDown = _UI.Q<VisualElement>("LearningRate").Q<VisualElement>("ImageDown");
        var learningRateUp = _UI.Q<VisualElement>("LearningRate").Q<VisualElement>("ImageUp");
        var decayRateDown = _UI.Q<VisualElement>("DecayRate").Q<VisualElement>("ImageDown");
        var decayRateUp = _UI.Q<VisualElement>("DecayRate").Q<VisualElement>("ImageUp");
        var maxStepsDown = _UI.Q<VisualElement>("MaxSteps").Q<VisualElement>("ImageDown");
        var maxStepsUp = _UI.Q<VisualElement>("MaxSteps").Q<VisualElement>("ImageUp");
        var maxEpisodesDown = _UI.Q<VisualElement>("MaxEpisodes").Q<VisualElement>("ImageDown");
        var maxEpisodesUp = _UI.Q<VisualElement>("MaxEpisodes").Q<VisualElement>("ImageUp");

        learningRateDown.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Images/Settings/learningRateDown"));
        learningRateUp.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Images/Settings/learningRateUp"));
        decayRateDown.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Images/Settings/explorationDown"));
        decayRateUp.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Images/Settings/explorationUp"));
        maxStepsDown.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Images/Settings/stepsDown"));
        maxStepsUp.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Images/Settings/stepsUp"));
        maxEpisodesDown.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Images/Settings/episodesDown"));
        maxEpisodesUp.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Images/Settings/episodesUp"));

        // labels
        _UI.Q<VisualElement>("LearningRate").Q<Label>("SettingsLabel").text = "Learning rate";
        _UI.Q<VisualElement>("DecayRate").Q<Label>("SettingsLabel").text = "Exploration rate";
        _UI.Q<VisualElement>("MaxSteps").Q<Label>("SettingsLabel").text = "Steps Per cycle";
        _UI.Q<VisualElement>("MaxEpisodes").Q<Label>("SettingsLabel").text = "Number of cycles";

    }

    #region EnableDisable
    public void EnableRewardAdjusters()
    {
        var rewardAdjusters = _rewardContainer.Query("RewardAdjuster").ToList();

        foreach (var adjuster in rewardAdjusters)
        {
            adjuster.Q<Slider>().SetEnabled(true);
        }
    }

    public void DisableRewardAdjusters()
    {
        var rewardAdjusters = _rewardContainer.Query("RewardAdjuster").ToList();

        foreach (var adjuster in rewardAdjusters)
        {
            adjuster.Q<Slider>().SetEnabled(false);
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

    public void EnableTrainingSettings()
    {
        EnableLearningRate();
        EnableDecayRate();
        EnableMaxSteps();
        EnableMaxEpisodes();
    }

    public void DisableTrainingSettings()
    {
        DisableLearningRate();
        DisableDecayRate();
        DisableMaxSteps();
        DisableMaxEpisodes();
    }

    public void EnableLearningRate()
    {
        learningRate.Q<Slider>().SetEnabled(true);
    }

    public void DisableLearningRate()
    {
        learningRate.Q<Slider>().SetEnabled(false);
    }

    public void EnableDecayRate()
    {
        decayRate.Q<Slider>().SetEnabled(true);
    }

    public void DisableDecayRate()
    {
        decayRate.Q<Slider>().SetEnabled(false);
    }

    public void EnableMaxSteps()
    {
        maxSteps.Q<Slider>().SetEnabled(true);
    }

    public void DisableMaxSteps()
    {
        maxSteps.Q<Slider>().SetEnabled(false);
    }

    public void EnableMaxEpisodes()
    {
        maxEpisodes.Q<Slider>().SetEnabled(true);
    }

    public void DisableMaxEpisodes()
    {
        maxEpisodes.Q<Slider>().SetEnabled(false);
    }
    #endregion

    string GetLevelName()
    {
        switch (_rlLevel)
        {
            case RLLevel.level1:
                return "StageTwo_1_";
            case RLLevel.level2:
                return "StageTwo_2_";
            case RLLevel.level3:
                return "StageTwo_3_";
        }

        return "";
    }
    public void ShowEndScreen()
    {
        var endScreen = _UI.Q<VisualElement>("EndScreen");
        endScreen.style.display = DisplayStyle.Flex;
        endScreen.RemoveFromClassList("panel-up");
    }
    public void OnSurveyButtonClicked()
    {
        if (ActivityTracker.Instance != null)
        {
            Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSffHAoJXE4RetOsmDZG0FycdND9QlhSbru142JqOCFz9zDUAQ/viewform?usp=pp_url&entry.978412280=" + ActivityTracker.Instance.GetSessionId());

        }

        Application.Quit();
    }
    public void HideEvaluationStatus()
    {
        evaluationStatus.style.display = DisplayStyle.None;
    }
    public void ShowEvaluationStatus()
    {
        Label statusValue = evaluationStatus.Q<Label>("StatusValue");
        var evaluatingStages = new HashSet<GameStage> {
            GameStage.RLOneStarted,
            GameStage.RLTwoStarted,
            GameStage.RLThreeStarted
        };
        var evaluatingStagesGood = new HashSet<GameStage> {
            GameStage.RLOneCompletedGood,
            GameStage.RLTwoCompletedGood,
            GameStage.RLThreeCompletedGood
        };
        var successRate = EvaluationController().GetSuccessRate();
        if (evaluatingStages.Contains(StateManager.Instance.CurrentStage))
        {
            statusValue.text = "Evaluation in progress...";
            evaluationStatus.style.display = DisplayStyle.Flex;
        }
        if (evaluatingStagesGood.Contains(StateManager.Instance.CurrentStage))
        {
            if (successRate >= 0.6f)
            {
                statusValue.text = "Excellent! Your robot is performing optimally.";
                statusValue.style.color = new StyleColor(new Color32(0x21, 0x63, 0x4F, 0xFF));
            }
            else
            {
                statusValue.text = "Reasonable performance. Your robot is showing stable results.";
                statusValue.style.color = new StyleColor(Color.yellow);
            }


        }
        else
        {
            statusValue.text = "Poor performance. Your robot is not learning effectively.";
            statusValue.style.color = new StyleColor(new Color32(0x82, 0x3A, 0x30, 0xFF));
        }
        evaluationStatus.style.display = DisplayStyle.Flex;
    }
}
