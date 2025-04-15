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

    Dictionary<TileType, Sprite> _tileSpritesDict = new();


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
        _rewardContainer = workshopPanel.Q<VisualElement>("RewardContainer");
        _start = workshopPanel.Q<Button>("Start");
        _speedUp = workshopPanel.Q<Button>("SpeedUp");
        _start.RegisterCallback<ClickEvent>(StartTrainingHandler);
        _speedUp.RegisterCallback<ClickEvent>(SpeedUpHandler);
        workshopPanel.style.display = DisplayStyle.Flex;
        workshopPanel.AddToClassList("panel-up");

        helpPanel = _UI.Q<VisualElement>("HelpPanel");
        helpPanel.style.display = DisplayStyle.Flex;

        tutorialPanel.AddToClassList("opacity-none");

        workshopOpenButton = _UI.Q<Button>("WorkshopOpenButton");
        workshopOpenButton.clicked += OnWorkshopOpenButtonClicked;
        workshopOpenButton.tooltip = "Open Workshop";
        workshopCloseButton = workshopPanel.Q<Button>("WorkshopCloseButton");
        workshopCloseButton.clicked += OnWorkshopCloseButtonClicked;
        evaluationOpenButton = _UI.Q<Button>("EvaluationOpenButton");
        evaluationOpenButton.clicked += OnEvaluationOpenButtonClicked;
        evaluationCloseButton = _UI.Q<Button>("EvaluationCloseButton");
        evaluationCloseButton.clicked += OnEvaluationCloseButtonClicked;

        LoadRewardAdjusters();
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
        evaluationPanel.RemoveFromClassList("panel-up");
        evaluationOpenButton.AddToClassList("opacity-none");
    }
    public void OnEvaluationCloseButtonClicked()
    {
        evaluationPanel.AddToClassList("panel-up");
        evaluationOpenButton.RemoveFromClassList("opacity-none");
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

            slider.RegisterValueChangedCallback(evt => SetTileRewardHandler(evt, tile, label));

            _rewardContainer.Add(rewardAdjuster);
        }
    }

    void SetTileRewardHandler(ChangeEvent<float> evt, TileType tile, Label label)
    {
        _manager.SetReward(tile, evt.newValue);
        label.text = evt.newValue.ToString();
    }

    void StartTrainingHandler(ClickEvent evt) 
    {
        DisableRewardAdjusters();
        _manager.StartTraining();
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

    public void LoadLevel()
    {
        SceneManager.LoadScene(_nextScene.name);
    }
}
