using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class QAgent : RLAgent
{
    Dictionary<State, Dictionary<Action, float>> _qTable;

    void Start()
    {
        _currentTile = controller.startingTile;
        _prevTile = _currentTile;
        _qTable = new();

        _rewards[TileType.Normal] = 0;
        _rewards[TileType.Wall] = -1;
        _rewards[TileType.Dangerous] = 0;
        _rewards[TileType.Enemy] = 0;
        _rewards[TileType.Goal] = 0;
        _rewards[TileType.Collectible] = 0;
        _rewards[TileType.Buff] = 0;
    }

    void Update()
    {
        if (!_activated) return;

        if (controller.IsDead || _finishedEpoch)
        {
            _finishedEpoch = true;
            return;
        }

        if (!_calculatingMove && !controller.IsMoving())
        {
            _calculatingMove = true;
            _currentTile = controller.currentTile;

            var state = GetState(_currentTile);

            var action = GetAction(state);

            var nextTile = _currentTile.GetAdjecentTile(action);
            var nextState = GetState(nextTile);

            var reward = GetReward(nextTile);
            totalReward += reward;
            currentEpochReward += reward;

            UpdateQTable(state, nextState, action, reward);

            _prevTile = _currentTile;
            controller.MoveToSelectedAction(action);

            totalStepCount++;
            currentEpochStepCount++;
            epsilon = _epsilonMin + (1.0f - _epsilonMin) * Mathf.Exp(-decayRate * totalStepCount);
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

        if (!_calculatingMove)
        {
            if (CompletedTask())
            {
                totalTaskCompleted++;
                _finishedEpoch = true;
            }

            if ((currentEpochStepCount >= maxSteps) && !_finishedEpoch) _finishedEpoch = true;
        }
    }

    override public Action GetAction(State state)
    {
        var possibleActions = controller.GetPossibleActions();

        if (possibleActions.Count() > 1)
        {
            possibleActions = possibleActions
                .Where(act => _currentTile.GetAdjecentTile(act).point.position != _prevTile.point.position)
                .ToList();
        }

        // Choose action using ε-greedy strategy
        Random.InitState(DateTime.Now.Millisecond);

        Action chosenAction = possibleActions[Random.Range(0, possibleActions.Count)];
        if (Random.value < epsilon || !_qTable.ContainsKey(state)) // Explore
        {
            return chosenAction;
        }

        var actionValues = _qTable[state];
        var qValue = 0f;
        foreach (var act in possibleActions)
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
        float newQ = oldQ + learningRate * (reward + _discountFactor * maxFutureQ - oldQ);

        // Update Q-table
        _qTable[prevState][action] = newQ;
    }

    public override void ResetModel()
    {
        base.ResetModel();
        _qTable.Clear();
    }

}
