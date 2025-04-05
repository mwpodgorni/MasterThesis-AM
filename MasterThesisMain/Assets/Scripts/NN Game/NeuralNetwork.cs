using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using Newtonsoft.Json;
public class NeuralNetwork
{
    public Layer inputLayer;
    public Layer outputLayer;
    public List<Layer> hiddenLayers = new List<Layer>();

    public float expectedOutput;
    public float actualOutput;

    public int epoch;
    public float loss;
    float avgLoss = 0.0f;


    public NeuralNetwork()
    {
        epoch = 0;
        inputLayer = new Layer();
        outputLayer = new Layer();
    }


    public void AddHiddenLayer()
    {
        RemoveWeights();

        Layer layer = new Layer();

        hiddenLayers.Add(layer);
        InitializeWeights();
    }

    public void RemoveHiddenLayer()
    {
        if (hiddenLayers.Count <= 0) return;

        RemoveWeights();

        var layer = hiddenLayers[hiddenLayers.Count - 1];

        hiddenLayers.Remove(layer);
        InitializeWeights();
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
            float error = expected[i] - output[i];
            node.gradient = error * (node.value * (1 - node.value));
        }

        // Compute hidden layer gradients (in reverse order)
        for (int l = hiddenLayers.Count - 1; l >= 0; l--)
        {
            Layer layer = hiddenLayers[l];
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

        // Update weights and biases for hidden layers
        foreach (Layer layer in hiddenLayers)
        {
            foreach (Node node in layer.nodes)
            {
                foreach (Weight w in node.weightsIn)
                {
                    w.weight += GP.Instance.learningRate * node.gradient * w.from.value;
                }
                node.bias += GP.Instance.learningRate * node.gradient;
            }
        }

        // Update weights and biases for output layer
        foreach (Node node in outputLayer.nodes)
        {
            foreach (Weight w in node.weightsIn)
            {
                w.weight += GP.Instance.learningRate * node.gradient * w.from.value;
            }
            node.bias += GP.Instance.learningRate * node.gradient;
        }
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

    }

    public void TrainNetwork(int epoch, float learningRate)
    {
        var trainingSet = JsonConvert.DeserializeObject<TrainingSet>(GP.GetFirstMiniGameDataset().text);
        if (trainingSet.data != null && trainingSet.data.Length > 0)
        {
            Debug.Log("Training data loaded");
        }
        else
        {
            Debug.LogError("Training data is empty or null.");
        }

        for (int i = 0; i < epoch; i++)
        {
            var set = trainingSet.data[Random.Range(0, trainingSet.data.Length)];

            var inputs = set.input;
            var expectedOutput = set.expected;

            var actualOutput = ForwardPass(inputs);
            Debug.Log($"Actual output length: {actualOutput.Length}, Expected output length: {expectedOutput.Length}");
            if (expectedOutput.Length != actualOutput.Length)
            {
                Debug.LogError($"Length mismatch: expectedOutput.Length = {expectedOutput.Length}, actualOutput.Length = {actualOutput.Length}");
                return; // Stop execution or handle it properly
            }
            // Compute Mean Squared Error (MSE)
            float loss = 0;
            for (int k = 0; k < expectedOutput.Length; k++)
            {
                loss += Mathf.Pow(expectedOutput[k] - actualOutput[k], 2);
            }
            loss /= expectedOutput.Length;
            avgLoss += loss;

            BackPropagate(actualOutput, expectedOutput);

        }

        avgLoss /= epoch;
        loss = avgLoss;
        avgLoss = 0f;
    }

    public void InitializeWeights()
    {
        RemoveWeights();

        List<Layer> allLayers = new List<Layer>();
        allLayers.Add(inputLayer);
        allLayers.AddRange(hiddenLayers);
        allLayers.Add(outputLayer);

        for (int l = 0; l < allLayers.Count - 1; l++)
        {
            foreach (var fromNode in allLayers[l].nodes)
            {
                foreach (var toNode in allLayers[l + 1].nodes)
                {
                    var weight = new Weight(fromNode, toNode);
                    fromNode.weightsOut.Add(weight);
                    toNode.weightsIn.Add(weight);
                }
            }
        }
    }
    public int GetLayerCount()
    {
        return hiddenLayers.Count;
    }
    public float GetLoss()
    {
        return loss;
    }
    public float GetExpectedOutput()
    {
        return expectedOutput;
    }
    public float GetActualOutput()
    {
        return actualOutput;
    }
    public void AddHiddenLayerNode(int index)
    {
        if (hiddenLayers.Count <= 0) return;

        hiddenLayers[index].AddNode();
        InitializeWeights();
    }
    public void AddInputLayerNode()
    {
        inputLayer.AddNode();
        InitializeWeights();
    }
    public void RemoveInputLayerNode()
    {
        inputLayer.RemoveNode();
        InitializeWeights();
    }
    public void AddOutputLayerNode()
    {
        outputLayer.AddNode();
        InitializeWeights();
    }
    public void RemoveOutputLayerNode()
    {
        outputLayer.RemoveNode();
        InitializeWeights();
    }

    public struct TrainingSet
    {
        public TrainingData[] data;
    }

    public struct TrainingData
    {
        public float[] input;
        public float[] expected;
    }
}
