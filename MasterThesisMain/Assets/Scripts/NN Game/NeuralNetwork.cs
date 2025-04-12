using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using Newtonsoft.Json;
public class NeuralNetwork
{
    // tracked variables
    public int finishedCycles;
    public float finalAverageLoss;
    public int correctPredictions;
    public int errorLessThan005;
    public int error005to01;
    public int errorGreaterThan01;
    public List<float> lossPerStep = new();
    // ---
    public Layer inputLayer;
    public Layer outputLayer;
    public List<Layer> hiddenLayers = new List<Layer>();

    public float expectedOutput;
    public float actualOutput;

    public float loss;
    float avgLoss = 0.0f;

    float learningRate = 0.001f;
    public Action<EvaluationData> OnEvaluationUpdate;
    public NeuralNetwork()
    {
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
                    // Debug.Log("Updating weight: " + w.weight);
                    w.weight += learningRate * node.gradient * w.from.value;
                }
                node.bias += learningRate * node.gradient;
            }
        }

        // Update weights and biases for output layer
        foreach (Node node in outputLayer.nodes)
        {
            foreach (Weight w in node.weightsIn)
            {
                w.weight += learningRate * node.gradient * w.from.value;
            }
            node.bias += learningRate * node.gradient;
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
        this.learningRate = learningRate;

        var trainingSet = JsonConvert.DeserializeObject<TrainingSet>(GP.GetFirstMiniGameDataset().text);
        if (trainingSet.data == null || trainingSet.data.Length == 0)
        {
            Debug.LogError("Training data is empty or null.");
            return;
        }

        correctPredictions = 0;
        errorLessThan005 = 0;
        error005to01 = 0;
        errorGreaterThan01 = 0;
        lossPerStep.Clear();

        float totalLoss = 0f;
        avgLoss = 0f;

        for (int i = 0; i < epoch; i++)
        {
            var set = trainingSet.data[Random.Range(0, trainingSet.data.Length)];

            var inputs = set.input;
            var expectedOutput = set.expected;

            // Perform forward pass
            var actualOutput = ForwardPass(inputs);

            // Ensure output and expected match in length
            if (expectedOutput.Length != actualOutput.Length)
            {
                Debug.LogError($"Length mismatch: expectedOutput.Length = {expectedOutput.Length}, actualOutput.Length = {actualOutput.Length}");
                return;
            }

            // Calculate loss and error
            Debug.Log($"actual output: {actualOutput[0]}, {actualOutput[1]}, {actualOutput[2]}");
            Debug.Log($"expected output: " + expectedOutput[0] + ", " + expectedOutput[1] + ", " + expectedOutput[2]);
            float loss = CalculateLoss(actualOutput, expectedOutput);
            float error = 0f;
            for (int k = 0; k < expectedOutput.Length; k++)
            {
                error += Mathf.Abs(expectedOutput[k] - actualOutput[k]);
            }
            error /= expectedOutput.Length;
            Debug.Log($"error: {error}");
            lossPerStep.Add(loss);
            avgLoss += loss;

            // Track predictions
            if (error < 0.05f)
            {
                errorLessThan005++;
                correctPredictions++;
            }
            else if (error < 0.1f)
            {
                error005to01++;
            }
            else
            {
                errorGreaterThan01++;
            }

            // Perform backpropagation
            BackPropagate(actualOutput, expectedOutput);

            totalLoss += loss;

            // Periodic evaluation update
            if ((i + 1) % 10 == 0)
            {
                OnEvaluationUpdate?.Invoke(
                    new EvaluationData
                    {
                        finishedCycles = finishedCycles + i + 1,
                        learningRate = learningRate,
                        finalAverageLoss = avgLoss / (i + 1),
                        correctPredictions = correctPredictions,
                        errorLow = errorLessThan005,
                        errorMid = error005to01,
                        errorHigh = errorGreaterThan01,
                        lossData = new List<float>(lossPerStep)
                    }
                );
            }
        }

        finalAverageLoss = avgLoss / epoch;
        loss = finalAverageLoss;
        finishedCycles += epoch;
        avgLoss = 0f;

        Debug.Log($"Training completed after {epoch} epochs.");
        Debug.Log($"Finished Cycles: {finishedCycles}");
        Debug.Log($"Final Average Loss: {finalAverageLoss}");
        Debug.Log($"Correct Predictions (<0.05): {correctPredictions}");
        Debug.Log($"Errors 0.05–0.1: {error005to01}");
        Debug.Log($"Errors >0.1: {errorGreaterThan01}");
    }
    public float CalculateLoss(float[] outputs, float[] targets)
    {
        float loss = 0f;
        for (int i = 0; i < outputs.Length; i++)
        {
            float error = outputs[i] - targets[i];
            loss += error * error;
        }
        return loss / outputs.Length;
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
    public void RemoveHiddenLayerNode(int index)
    {
        if (hiddenLayers.Count <= 0) return;

        Debug.Log("NN: Removing hidden layer node" + index);
        hiddenLayers[index].RemoveNode();
        InitializeWeights();
    }
    public void AddInputLayerNode()
    {
        inputLayer.AddNode();
        InitializeWeights();
    }
    public void RemoveInputLayerNode()
    {
        Debug.Log("NN: Removing input layer node");
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
        Debug.Log("NN: Removing output layer node");
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
    public int GetInputLayerCount()
    {
        return inputLayer.nodes.Count;
    }
    public int GetOutputLayerCount()
    {
        return outputLayer.nodes.Count;
    }
    public int[] GetHiddenLayerCounts()
    {
        int[] counts = new int[hiddenLayers.Count];
        for (int i = 0; i < hiddenLayers.Count; i++)
        {
            counts[i] = hiddenLayers[i].nodes.Count;
        }
        return counts;
    }
    public bool IsNetworkValid()
    {
        Debug.Log("Is Network Valid?");
        Debug.Log("Input Layer Count: " + inputLayer.nodes.Count);
        Debug.Log("Output Layer Count: " + outputLayer.nodes.Count);
        Debug.Log("Hidden Layer Count: " + hiddenLayers.Count);
        foreach (var layer in hiddenLayers)
        {
            Debug.Log("Hidden LayerNode Count: " + layer.nodes.Count);
        }
        if (inputLayer.nodes.Count == 0) return false;
        if (outputLayer.nodes.Count == 0) return false;
        if (hiddenLayers.Count == 0) return false;

        foreach (var layer in hiddenLayers)
        {
            if (layer.nodes.Count == 0) return false;
        }

        return true;
    }
    public void ResetNetwork()
    {
        inputLayer = new Layer();
        outputLayer = new Layer();
        hiddenLayers.Clear();
        loss = 0.0f;
        avgLoss = 0.0f;
    }
}
public struct EvaluationData
{
    public int finishedCycles;
    public float learningRate;
    public float finalAverageLoss;
    public int correctPredictions;
    public int errorLow;
    public int errorMid;
    public int errorHigh;
    public List<float> lossData;
}
