using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;


public class Node : MonoBehaviour
{
    public List<Weight> weightsIn;
    public List<Weight> weightsOut;

    public float value;
    public float bias;
    public float gradient;

    Func<float, float> _activationFunc;

    [SerializeField] Parameters _parameters;

    public void Start()
    {
        bias = Random.Range(
            _parameters.BiasRange.Item1,
            _parameters.BiasRange.Item2
        );

        _activationFunc = _parameters.ActivationFunction;
    }

    public void SetActivationFunc(Func<float, float> func)
    {
        _activationFunc = func;
    }

    public void Activate()
    {
        value = _activationFunc(CalculateWeightedSum());
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
