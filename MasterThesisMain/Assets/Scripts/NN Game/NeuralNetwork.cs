using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
using UnityEditor.PackageManager;

public class NeuralNetwork : MonoBehaviour
{
    public Layer inputLayer;
    public Layer outputLayer;
    public List<Layer> hiddenLayers = new List<Layer>();

    public float expectedOutput;
    public float actualOutput;

    public int epoch;
    public float loss;
    float avgLoss = 0.0f;


    [SerializeField] Transform _weightPanel;

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
    }

    public void RemoveHiddenLayer()
    {
        if (hiddenLayers.Count <= 0) return;

        RemoveWeights();

        var layer = hiddenLayers[hiddenLayers.Count - 1];

        hiddenLayers.Remove(layer);
    }

    public float ForwardPass(float[] inputs)
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

        var outputNode = outputLayer.nodes[0];

        outputNode.Activate();

        return outputNode.value;
    }

    public void BackPropagate(float output, float expected)
    {
        for (int i = 0; i < outputLayer.nodes.Count; i++)
        {
            Node node = outputLayer.nodes[i];
            float error = expected - node.value;
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
                    w.weight += GP.Instance.learningRate * w.to.gradient * w.from.value;
                }
                node.bias += GP.Instance.learningRate * node.gradient;
            }
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

        for (int i = 0; i < epoch; i++)
        {
            var selectedInput = Random.Range(0, inputLayer.nodes.Count);
            var inputs = new float[inputLayer.nodes.Count];
            inputs[selectedInput] = 1;

            expectedOutput = selectedInput;
            actualOutput = ForwardPass(inputs);
            avgLoss += Mathf.Pow(expectedOutput - actualOutput, 2);

            BackPropagate(actualOutput, expectedOutput);

        }

        avgLoss /= epoch;
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
    }
    public void AddInputLayerNode()
    {
        inputLayer.AddNode();
    }
    public void RemoveInputLayerNode()
    {
        inputLayer.RemoveNode();
    }
    public void AddOutputLayerNode()
    {
        outputLayer.AddNode();
    }
    public void RemoveOutputLayerNode()
    {
        outputLayer.RemoveNode();
    }
}
