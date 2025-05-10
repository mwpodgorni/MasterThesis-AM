using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/NN Parameters")]
public class Parameters : ScriptableObject
{
    [Header("Neural Network Parameters")]
    public float maxWeight = 1f;
    public float minWeight = 0f;

    public float maxBias = 1f;
    public float minBias = -0f;

    public int maxNodes = 5;
    public int maxLayers = 4;

    [Header("Hyper Parameters")]
    public float learningRate = 0.1f;
    public float gradient = 0.1f;

    [SerializeField]
    ActivationFunctionType activationFunc = ActivationFunctionType.Sigmoid;

    public GameObject nodePrefab;
    public GameObject weightPrefab;
    public GameObject layerPrefab;

    public (float, float) WeightRange
    {
        get { return (minWeight, maxWeight); }
    }

    public (float, float) BiasRange
    {
        get { return (minBias, maxBias); }
    }

    public Func<float, float> ActivationFunction
    {
        get { return ActivationFunctions.GetFunction(activationFunc); }
    }
}

public static class ActivationFunctions
{
    // Binary Step Function
    public static float BinaryStep(float x) => x >= 0 ? 1f : 0f;

    // Sigmoid Function
    public static float Sigmoid(float x) => 1f / (1f + MathF.Exp(-x));

    // Tanh (Hyperbolic Tangent) Function
    public static float Tanh(float x) => MathF.Tanh(x);

    // ReLU (Rectified Linear Unit)
    public static float ReLU(float x) => MathF.Max(0, x);

    public static Func<float, float> GetFunction(ActivationFunctionType type)
    {
        return type switch
        {
            ActivationFunctionType.BinaryStep => BinaryStep,
            ActivationFunctionType.Sigmoid => Sigmoid,
            ActivationFunctionType.Tanh => Tanh,
            ActivationFunctionType.ReLU => ReLU,
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Unknown activation function")
        };
    }
}

public enum ActivationFunctionType
{
    BinaryStep,
    Sigmoid,
    Tanh,
    ReLU
}
