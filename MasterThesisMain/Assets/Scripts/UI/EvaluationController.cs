using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
    }
    public void UpdateEvaluationData(EvaluationData data)
    {
        Debug.Log($"Updating evaluation data: {data}");
        finishedCyclesValue.text = data.finishedCycles.ToString();
        learningRateValue.text = data.learningRate.ToString("F4");
        finalAverageLossValue.text = data.finalAverageLoss.ToString("F4");
        correctPredictionsValue.text = data.correctPredictions.ToString();
        errorLowValue.text = data.errorLow.ToString();
        errorMidValue.text = data.errorMid.ToString();
        errorHighValue.text = data.errorHigh.ToString();
        chart.data = DownsampleLossData(data.lossData, 100);
    }
    private List<float> DownsampleLossData(List<float> lossData, int maxDataPoints = 1000)
    {
        if (lossData.Count <= maxDataPoints) return lossData;

        int step = Mathf.CeilToInt((float)lossData.Count / maxDataPoints);
        List<float> downsampledData = new List<float>();

        for (int i = 0; i < lossData.Count; i += step)
        {
            downsampledData.Add(lossData[i]);
        }

        return downsampledData;
    }
}
