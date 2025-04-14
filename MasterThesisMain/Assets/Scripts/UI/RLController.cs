using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RLController : MonoBehaviour
{
    Button _start;
    Label _speedUp;
    VisualElement _UI;
    VisualElement _rewardContainer;

    [SerializeField] VisualTreeAsset _rewardAdjusterTemplate;
    [SerializeField] RLManager _manager;

    [SerializeField] 

    // Start is called before the first frame update
    void Start()
    {
        _UI = GetComponent<UIDocument>().rootVisualElement;
        _rewardContainer = _UI.Q<VisualElement>("RewardContainer");
        _start = _UI.Q<Button>("Start");
        _speedUp = _UI.Q<Label>("SpeedUp");

        Debug.Log(_rewardContainer);
        LoadRewardAdjusters();

        _start.RegisterCallbackOnce<ClickEvent>(StartTrainingHandler);
        _speedUp.RegisterCallbackOnce<ClickEvent>(SpeedUpHandler);
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
}
