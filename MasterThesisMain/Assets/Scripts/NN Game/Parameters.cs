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

    [Header("Hyper Parameters")]
    public float learningRate = 0.1f;
    public float gradient = 0.1f;

    public List<Func<float, float>> activationFuncs = new List<Func<float, float>>()
    {
        ActivationFunctions.BinaryStep,
        ActivationFunctions.Sigmoid,
        ActivationFunctions.Tanh,
        ActivationFunctions.ReLU
    };

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
}
