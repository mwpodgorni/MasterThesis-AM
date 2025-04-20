using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class RLController : MonoBehaviour
{
    Button _start;
    Button _speedUp;
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
    public VisualElement helpPanel;

    [SerializeField] VisualTreeAsset _rewardAdjusterTemplate;
    [SerializeField] RLManager _manager;

    [SerializeField] SceneAsset _nextScene;
    [SerializeField] List<Sprite> _tileSprites;

    [SerializeField] RLLevel _rlLevel = RLLevel.level1;

    Dictionary<TileType, Sprite> _tileSpritesDict = new();

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
        _speedUp = workshopPanel.Q<Button>("SpeedUp");
        _start.RegisterCallback<ClickEvent>(StartTrainingHandler);
        _stop.RegisterCallback<ClickEvent>(StopTrainingHandler);
        _speedUp.RegisterCallback<ClickEvent>(SpeedUpHandler);
        _help.RegisterCallback<ClickEvent>(evt => HelpController().ShowHelp("rewardAdjuster"));
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

        LoadRewardAdjusters();
        DisableStopButton();
        DisableSpeedButton();
        StartCoroutine(StartTutorial(GetTutorialText));
    }

    IEnumerator StartTutorial(string name)
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Starting tutorial");
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
        EvaluationController().UpdateEvaluationData(_manager.currentEval);
        evaluationPanel.RemoveFromClassList("panel-up");
        evaluationOpenButton.AddToClassList("opacity-none");
    }
    public void OnEvaluationCloseButtonClicked()
    {
        evaluationPanel.AddToClassList("panel-up");
        evaluationOpenButton.RemoveFromClassList("opacity-none");
    }

    public void OnLevelFinished(bool solved)
    {
        if (solved) TutorialController().AddToEvent(LoadLevel);
        StartCoroutine(StartTutorial(GetSolvedText(solved)));

        DisableStopButton();
        EnableStartButton();
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
        DisableRewardAdjusters();
        DisableStartButton();
        EnableStopButton();
        EnableSpeedButton();
        _manager.StartTraining();
    }

    void StopTrainingHandler(ClickEvent evt)
    {
        EnableRewardAdjusters();
        EnableStartButton();
        DisableStopButton();
        DisableSpeedButton();
        _manager.StopTraining();
    }

    void SpeedUpHandler(ClickEvent evt)
    {
        _manager.ToggleSpeed();
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

    public void EnableSpeedButton()
    {
        _speedUp.SetEnabled(true);
    }
    
    public void DisableSpeedButton()
    {
        _speedUp.SetEnabled(false);
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

    public void LoadLevel()
    {
        SceneManager.LoadScene(_nextScene.name);
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
