using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

public class Node
{
    public List<Weight> weightsIn = new List<Weight>();
    public List<Weight> weightsOut = new List<Weight>();

    public float value;
    public float bias;
    public float gradient;

    private Func<float, float> _activationFunc;

    public Node()
    {

        bias = Random.Range(GP.Instance.BiasRange.Item1, GP.Instance.BiasRange.Item2);
        _activationFunc = GP.Instance.ActivationFunction;

    }

    public void SetActivationFunc(Func<float, float> func)
    {
        _activationFunc = func;
    }

    public void Activate()
    {
        value = _activationFunc(CalculateWeightedSum());
        value = _activationFunc != null ? _activationFunc(CalculateWeightedSum()) : CalculateWeightedSum();
    }

    float CalculateWeightedSum()
    {
        float sum = 0;

        foreach (var weight in weightsIn)
        {
            sum += weight.GetWeightSum();
        }

        return sum + bias;
    }

    public void ClearWeights()
    {
        weightsIn.Clear();
        weightsOut.Clear();
    }

}
