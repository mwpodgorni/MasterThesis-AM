using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class QAgent : RLAgent
{
    Dictionary<State, Dictionary<Action, float>> _qTable;

    // Start is called before the first frame update
    void Start()
    {
        _qTable = new();

        _rewards[TileType.Normal] = 0;
        _rewards[TileType.Wall] = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_activated) return;

        if (_controller.IsDead || _finishedEpoch)
        {
            _finishedEpoch = true;
            return;
        }

        if (!_calculatingMove && !_controller.IsMoving())
        {
            var state = GetState(_controller.currentTile);

            var action = GetAction(state);

            var nextTile = _controller.currentTile.GetAdjecentTile(action);
            var nextState = GetState(nextTile);

            var reward = GetReward(nextTile);
            totalReward += reward;

            UpdateQTable(state, nextState, action, reward);

            _controller.MoveToSelectedAction(action);

            totalStepCount++;
            _epsilon = _epsilonMin + (1.0f - _epsilonMin) * Mathf.Exp(-_decayRate * totalStepCount);
        }

        if (_calculatingMove && _timer < _waitTime)
        {
            _timer += Time.deltaTime;
        }
        else
        {
            _calculatingMove = false;
            _timer = 0f;
        }

        if (totalStepCount % maxSteps == 0)
        {
            _finishedEpoch = true;
        }
    }

    override public Action GetAction(State state)
    {
        var possibleActions = _controller.GetPossibleActions();

        // Choose action using ε-greedy strategy
        Random.InitState(DateTime.Now.Millisecond);

        Action chosenAction = possibleActions[Random.Range(0, possibleActions.Count)];
        if (Random.value < _epsilon || !_qTable.ContainsKey(state)) // Explore
        {
            return chosenAction;
        }

        var actionValues = _qTable[state];
        var qValue = 0f;
        foreach (var act in _actions)
        {
            float currentQValue;

            if (!actionValues.TryGetValue(act, out currentQValue)) continue;

            if (currentQValue > qValue)
            {
                qValue = currentQValue;
                chosenAction = act;
            }
        }
        
        return chosenAction;
    }

    void UpdateQTable(State prevState, State newState, Action action, float reward)
    {
        if (!_qTable.ContainsKey(prevState))
        {
            Debug.Log("New State Discovered");
            _qTable[prevState] = new Dictionary<Action, float>();
        }
        if (!_qTable[prevState].ContainsKey(action))
        {
            _qTable[prevState][action] = 0f; // Default Q-value
        }

        float oldQ = _qTable[prevState][action];
        float maxFutureQ = 0f;

        if (_qTable.ContainsKey(newState) && _qTable[newState].Count > 0)
        {
            maxFutureQ = _qTable[newState].Values.Max();
        }

        // Q-learning formula
        float newQ = oldQ + _learningRate * (reward + _discountFactor * maxFutureQ - oldQ);

        // Update Q-table
        _qTable[prevState][action] = newQ;
    }

    public override void ResetModel()
    {
        _qTable.Clear();
    }

}
