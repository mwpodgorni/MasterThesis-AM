using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NeuralNetwork : MonoBehaviour
{
    public Layer inputLayer;
    public Layer outputLayer;
    public List<Layer> hiddenLayers;

    public float expectedOutput;
    public float actualOutput;

    [SerializeField] Parameters _parameters;

    [SerializeField] TextMeshProUGUI _layerCount;
    [SerializeField] Transform _hiddenLayerPanel;
    [SerializeField] Transform _weightPanel;

    public void AddHiddenLayer()
    {
        var layer = Instantiate(_parameters.layerPrefab).GetComponent<Layer>();

        hiddenLayers.Add(layer);

        layer.transform.SetParent(_hiddenLayerPanel);
        _layerCount.text = hiddenLayers.Count.ToString();
    }

    public void RemoveHiddenLayer()
    {
        var layer = hiddenLayers[hiddenLayers.Count - 1];

        hiddenLayers.Remove(layer);
        Destroy(layer.gameObject);

        _layerCount.text = hiddenLayers.Count.ToString();
    }

    public void ConnectLayers(Layer from, Layer to)
    {
        foreach (Node nodeFrom in from.nodes)
        {
            foreach (Node nodeTo in to.nodes)
            {
                var weight = Instantiate(_parameters.weightPrefab).GetComponent<Weight>();

                weight.from = nodeFrom;
                weight.to = nodeTo;

                nodeFrom.weightsOut.Add(weight);
                nodeTo.weightsIn.Add(weight);
            }
        }
    }

    public void ForwardPass(float[] input)
    {

    }

    public void BackPropagate(float[] output)
    {

    }
}
