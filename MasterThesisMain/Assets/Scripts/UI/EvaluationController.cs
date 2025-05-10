using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;
public class EvaluationController : MonoBehaviour
{
    VisualElement ui;
    Label finishedCyclesValue;
    Label learningRateValue;
    Label finalAverageLossValue;

    Label correctPredictionsValue;
    Label errorLowValue;
    Label errorMidValue;
    Label errorHighValue;
    LineChart chart;

    void Start()
    {
    }
    private void OnEnable()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
        chart = ui.Q<LineChart>("LineChart");
        finishedCyclesValue = ui.Q<Label>("FinishedCyclesValue");
        learningRateValue = ui.Q<Label>("LearningRateValue");
        finalAverageLossValue = ui.Q<Label>("FinalAverageLossValue");
        correctPredictionsValue = ui.Q<Label>("CorrectPredictionsValue");
        errorLowValue = ui.Q<Label>("ErrorLowValue");
        errorMidValue = ui.Q<Label>("ErrorMidValue");
        errorHighValue = ui.Q<Label>("ErrorHighValue");
        // var seriesA = new List<float> { 1f, 2f, 3f, 2f, 1f };
        // var seriesB = new List<float> { 2f, 1f, 4f, 1f, 2f };
        // var seriesC = new List<float> { 3f, 3f, 1f, 3f, 3f };

        // dummy data
        // chart.datasets = new List<(List<float>, Color, string)> {
        //     (seriesA, Color.green, "Series A"),
        //     (seriesB, Color.yellow, "Series B"),
        //     (seriesC, Color.red, "Series C")
        // };
    }
    public void UpdateEvaluationData(EvaluationData data)
    {
        // Debug.Log($"Updating evaluation data: {data}");
        finishedCyclesValue.text = data.finishedCycles.ToString();
        learningRateValue.text = data.learningRate.ToString("F4");
        finalAverageLossValue.text = data.finalAverageLoss.ToString("F4");
        correctPredictionsValue.text = data.correctPredictions.ToString();
        errorLowValue.text = data.errorLow.ToString();
        errorMidValue.text = data.errorMid.ToString();
        errorHighValue.text = data.errorHigh.ToString();
        var downsampled = DownsampleLossData(data.lossData, 100);
        chart.datasets = new List<(List<float>, Color, string)> {
            (downsampled, new Color32(0x82, 0x3A, 0x30, 0xFF), "Loss Data"),
        };
        chart.Refresh();
    }
    private List<float> DownsampleLossData(List<float> lossData, int maxDataPoints = 1000)
    {
        if (lossData.Count <= maxDataPoints) return new List<float>(lossData);

        List<float> downsampled = new List<float>(maxDataPoints);
        int sectionSize = lossData.Count / maxDataPoints;

        for (int i = 0; i < maxDataPoints; i++)
        {
            int start = i * sectionSize;
            int end = Mathf.Min(start + sectionSize, lossData.Count);

            float sum = 0f;
            int count = 0;
            for (int j = start; j < end; j++)
            {
                sum += lossData[j];
                count++;
            }

            if (count > 0)
                downsampled.Add(sum / count);
        }

        return downsampled;
    }
    public int GetFinishedCycles()
    {
        return int.Parse(finishedCyclesValue.text);
    }
    public int GetCorrectPredictions()
    {
        return int.Parse(correctPredictionsValue.text);
    }
    public float GetFinalAverageLoss()
    {
        return float.Parse(finalAverageLossValue.text);
    }

    public int GetErrorLow()
    {
        return int.Parse(errorLowValue.text);
    }

    public int GetErrorMid()
    {
        return int.Parse(errorMidValue.text);
    }

    public int GetErrorHigh()
    {
        return int.Parse(errorHighValue.text);
    }
}
