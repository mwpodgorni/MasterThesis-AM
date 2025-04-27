using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RLEvaluationController : MonoBehaviour
{
    VisualElement ui;
    Label avgEpisodeReturn;
    Label successRate;
    Label completionTime;
    Label episodeCount;

    LineChart chart;

    void Start()
    {
    }
    private void OnEnable()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;

        ui = ui.Query<VisualElement>("EvaluationPanel");

        chart = ui.Q<LineChart>("LineChart");
        avgEpisodeReturn = ui.Q<Label>("EpisodeReturn");
        successRate = ui.Q<Label>("SuccessRate");
        completionTime = ui.Q<Label>("CompletionTime");
        episodeCount = ui.Q<Label>("EpisodeCount");
    }
    public void UpdateEvaluationData(RLEvaluationData data)
    {
        if (StateManager.Instance.CurrentStage == GameStage.RLTwoStarted
        || StateManager.Instance.CurrentStage == GameStage.RLThreeStarted
        )
        {
            RLController.Instance.ShowEvaluationOpenButton();
        }
        // Debug.Log($"Updating evaluation data: {data}");
        avgEpisodeReturn.text = data.avgEpisodeReturn.ToString();
        successRate.text = data.successRate.ToString();
        completionTime.text = data.completionTime.ToString();
        episodeCount.text = data.episodeCount.ToString();

        chart.datasets = new List<(List<float>, Color, string)> {
            (DownsampleData(data.episodeReward), new Color32(0x82, 0x3A, 0x30, 0xFF), "Episode Reward"),
            (DownsampleData(data.successRateRolling), new Color32(0xFF, 0xF7, 0x73, 0xFF), "Success Rate"),
            (DownsampleData(data.stepsToCompletion), new Color32(0x21, 0x63, 0x4F, 0xFF), "Steps to Completion")
        };
        chart.Refresh();
    }
    private List<float> DownsampleData(float[] lossData, int maxDataPoints = 1000)
    {
        Debug.Log($"Downsampling data: {lossData.Length} points to {maxDataPoints} max points.");
        if (lossData.Length <= maxDataPoints) return new List<float>(lossData);

        int step = Mathf.CeilToInt((float)lossData.Length / maxDataPoints);
        List<float> downsampledData = new List<float>();

        for (int i = 0; i < lossData.Length; i += step)
        {
            downsampledData.Add(lossData[i]);
        }

        return downsampledData;
    }
}
