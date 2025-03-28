using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
using UnityEditor.PackageManager;
using System;

public class NeuralNetwork : MonoBehaviour
{
    public Layer inputLayer;
    public Layer outputLayer;
    public List<Layer> hiddenLayers;

    public float expectedOutput;
    public float actualOutput;

    [SerializeField] Parameters _parameters;

    [SerializeField] TextMeshProUGUI _layerCount;
    [SerializeField] TextMeshProUGUI _expectedText;
    [SerializeField] TextMeshProUGUI _actualText;
    [SerializeField] TextMeshProUGUI _lossText;
    [SerializeField] Transform _hiddenLayerPanel;
    [SerializeField] Transform _weightPanel;
    [SerializeField] TMP_InputField _inputField;

    void Start()
    {
        hiddenLayers.AddRange(_hiddenLayerPanel.GetComponentsInChildren<Layer>());
        CreateWeights();
    }

    public void AddHiddenLayer()
    {
        if (hiddenLayers.Count >= _parameters.maxLayers) return;

        RemoveWeights();

        var layer = Instantiate(_parameters.layerPrefab).GetComponent<Layer>();

        hiddenLayers.Add(layer);

        layer.transform.SetParent(_hiddenLayerPanel);
        _layerCount.text = hiddenLayers.Count.ToString();

        CreateWeights();
    }

    public void RemoveHiddenLayer()
    {
        if (hiddenLayers.Count <= 0) return;

        RemoveWeights();

        var layer = hiddenLayers[hiddenLayers.Count - 1];

        hiddenLayers.Remove(layer);
        Destroy(layer.gameObject);

        _layerCount.text = hiddenLayers.Count.ToString();

        CreateWeights();
    }

    public void ConnectLayers(Layer from, Layer to)
    {
        foreach (Node nodeFrom in from.nodes)
        {
            foreach (Node nodeTo in to.nodes)
            {
                var weight = Instantiate(_parameters.weightPrefab).GetComponent<Weight>();

                weight.transform.SetParent(_weightPanel);

                weight.from = nodeFrom;
                weight.to = nodeTo;

                nodeFrom.weightsOut.Add(weight);
                nodeTo.weightsIn.Add(weight);
            }
        }
    }

    public float[] ForwardPass(float[] inputs)
    {
        for (int i = 0; i < inputLayer.nodes.Count; i++)
        {
            inputLayer.nodes[i].value = inputs[i];
        }

        foreach (var layer in hiddenLayers)
        {
            foreach (var node in layer.nodes)
            {
                node.Activate();
            }
        }

        var output = new float[outputLayer.nodes.Count];

        for (int i = 0; i < outputLayer.nodes.Count; i++)
        {
            outputLayer.nodes[i].Activate();
            output[i] = outputLayer.nodes[i].value;
        }

        return output;
    }

    public void BackPropagate(float[] output, float[] expected)
    {
        for (int i = 0; i < outputLayer.nodes.Count; i++)
        {
            Node node = outputLayer.nodes[i];
            float error = expected[i] - node.value;
            node.gradient = error * (node.value * (1 - node.value));
        }

        // Compute hidden layer gradients
        foreach (var layer in hiddenLayers)
        {
            foreach (Node node in layer.nodes)
            {
                float sumGradients = 0;
                foreach (Weight w in node.weightsOut)
                {
                    sumGradients += w.weight * w.to.gradient;
                }
                node.gradient = sumGradients * (node.value * (1 - node.value));
            }
        }

        // Update weights
        foreach (Layer layer in hiddenLayers)
        {
            foreach (Node node in layer.nodes)
            {
                foreach (Weight w in node.weightsIn)
                {
                    w.weight += _parameters.learningRate * w.to.gradient * w.from.value;
                }
                node.bias += _parameters.learningRate * node.gradient;
            }
        }
    }

    public void CreateWeights()
    {
        ConnectLayers(inputLayer, hiddenLayers[0]);

        for (var i = 0; i < hiddenLayers.Count - 1; i++)
        {
            ConnectLayers(hiddenLayers[i], hiddenLayers[i + 1]);
        }

        ConnectLayers(hiddenLayers[hiddenLayers.Count - 1], outputLayer);
    }

    public void RemoveWeights()
    {
        foreach (var node in inputLayer.nodes)
        {
            node.ClearWeights();
        }
        foreach (var node in outputLayer.nodes)
        {
            node.ClearWeights();
        }

        foreach (var layer in hiddenLayers)
        {
            foreach (var node in layer.nodes)
            {
                node.ClearWeights();
            }
        }

        if (_weightPanel.childCount > 0)
        {
            foreach (Transform child in _weightPanel.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void LayerUpdate(int i)
    {
        if (i > 0) CreateWeights();
        if (i <= 0) RemoveWeights();
    }

    public void TrainNN(int epoch, Tuple<float[], float[]>[] trainingData)
    {
        var avgLoss = 0.0f;

        for (int i = 0; i < epoch; i++)
        {
            var set = trainingData[Random.Range(0, trainingData.Length)];

            var input = set.Item1;
            var expectedOutput = set.Item2;

            var actualOutput = ForwardPass(input);

            var loss = 0f;

            for (int j = 0; j < expectedOutput.Length; j++)
            {
                var error = actualOutput[j] - expectedOutput[j];
                loss += error * error;
            }

            avgLoss += loss/expectedOutput.Length;

            BackPropagate(actualOutput, expectedOutput);

            _actualText.text = actualOutput.ToString();
            _expectedText.text = expectedOutput.ToString();
        }

        avgLoss /= epoch;

        _lossText.text = avgLoss.ToString();
    }
}
