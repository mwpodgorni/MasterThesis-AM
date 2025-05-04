using Alexwsu.EventChannels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RLAgent : MonoBehaviour
{
    [Header("Hyper Parameters")]
    [SerializeField] public float learningRate = 0.1f;   // Alpha (α)
    [SerializeField] protected float _discountFactor = 0.9f; // Gamma (γ);
    [SerializeField] public float decayRate = 0.01f;

    [SerializeField] public float epsilon = 1f;
    [SerializeField] protected float _epsilonMin = 0.01f;

    [SerializeField] public int maxSteps = 20;

    [Header("Properties")]
    public AgentController controller;
    public List<Task> tasks;
    public EventChannel taskCompletedChannel;

    [Header("Stats")]
    public float avgRewardPerEpoch = 0;
    public float totalReward = 0;
    public float currentEpochReward = 0;
    public int totalStepCount = 0;
    public float totalTaskCompleted = 0;
    public int currentEpochStepCount = 0;

    protected bool _calculatingMove = false;
    protected float _waitTime = 1f;
    protected float _timer = 0f;
    protected bool _finishedEpoch = false;

    protected Dictionary<TileType, float> _rewards = new Dictionary<TileType, float>();
    protected Action[] _actions = { Action.Up, Action.Down, Action.Left, Action.Right };

    protected bool _activated = false;
    protected Tile _currentTile;
    protected Tile _prevTile;

    public bool Activated { 
        get { return _activated; } 
        set { _activated = value; }
    }

    virtual public Action GetAction(State state)
    {
        var possibleActions = controller.GetPossibleActions();
        return possibleActions[Random.Range(0, possibleActions.Count)];
    }

    virtual public State GetState(Tile tile)
    {
        float[] obs = new float[4];

        for (int i = 0; i < _actions.Length; i++)
        {
            var adjecentTile = tile.GetAdjecentTile(_actions[i]);

            if (adjecentTile != null) obs[i] = adjecentTile.GetTileTypeToInt();
            else obs[i] = -2f;
        }

        var state = new State();
        state.obs = obs;

        return state;
    }

    virtual public float GetReward(Tile tile)
    {
        return _rewards[tile.GetTileType()];
    }

    virtual public void SetReward(TileType type, float value)
    {
        _rewards[type] = value;
    }

    virtual public void ResetAgent()
    {
        totalTaskCompleted = 0;
        controller.ResetAgent();
        _finishedEpoch = false;
        currentEpochReward = 0;
        currentEpochStepCount = 0;

        _currentTile = controller.startingTile;
        _prevTile = controller.startingTile;
    }

    virtual public void ResetModel()
    {
        totalStepCount = 0;
    }

    public bool FinishedEpoch 
    {
        get { return _finishedEpoch; }
    }


    public struct State
    {
        public float[] obs;

        public override bool Equals(object obj)
        {
            if (obj is State other)
            {
                return obs.SequenceEqual(other.obs);  // Compare arrays by their content
            }
            return false;
        }

        // Override GetHashCode to generate a hash based on the array's contents
        public override int GetHashCode()
        {
            return obs.Aggregate(17, (current, element) => current * 31 + element.GetHashCode());
        }
    }

    public bool CompletedTask()
    {
        foreach (var task in tasks)
        {
            if (!task.IsComplete()) return false;
        }

        taskCompletedChannel.Invoke(new Empty());
        return true;
    }
}
