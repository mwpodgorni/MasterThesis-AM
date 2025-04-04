using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class QAgent : MonoBehaviour
{
    Dictionary<State, Dictionary<Action, float>> _qTable;
    Action[] actions = { Action.Up, Action.Down, Action.Left, Action.Right };

    [Header("Hyper Parameters")]
    [SerializeField] float learningRate = 0.1f;   // Alpha (α)
    [SerializeField] float discountFactor = 0.9f; // Gamma (γ);
    [SerializeField] float decayRate = 0.01f;

    [SerializeField] float epsilon = 1f;
    [SerializeField] float epsilonMin = 0.01f;

    [SerializeField] int totalStepCount = 0;
    [SerializeField] int currentEpisodeSteps = 0;
    [SerializeField] int episodeCount = 0;
    [SerializeField] int maxSteps = 20;

    [Header("Properties")]
    [SerializeField] PlayerAgent _player;
    [SerializeField] Tile _goalTile;
    [SerializeField] Tile[] _tilesToReset;

    Dictionary<TileType, float> rewards = new Dictionary<TileType, float>();

    bool _calculatingMove = false;
    float _waitTime = 1f;
    float _timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        _qTable = new();

        rewards[TileType.Normal] = 0;
        rewards[TileType.Dangerous] = -1;
        rewards[TileType.Wall] = -1;
        rewards[TileType.Collectible] = 0.5f;
        rewards[TileType.Goal] = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_calculatingMove && !_player.IsMoving())
        {
            var state = GetState(_player.currentTile);

            var action = GetAction(state);

            var nextTile = _player.currentTile.GetAdjecentTile(action);
            var nextState = GetState(nextTile);

            var reward = GetReward(nextTile);

            UpdateQTable(state, nextState, action, reward);

            _player.MoveToSelectedAction(action);

            currentEpisodeSteps++;
            totalStepCount++;
            epsilon = epsilonMin + (1.0f - epsilonMin) * Mathf.Exp(-decayRate * totalStepCount);
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

        if (_player.IsDead() || currentEpisodeSteps % maxSteps == 0)
        {
            ResetLevel();
            episodeCount++;
            currentEpisodeSteps = 0;
        }
    }

    Action GetAction(State state)
    {
        var possibleActions = _player.GetPossibleActions();

        // Choose action using ε-greedy strategy
        Random.InitState(DateTime.Now.Millisecond);

        Action chosenAction = possibleActions[Random.Range(0, possibleActions.Count)];
        if (Random.value < epsilon || !_qTable.ContainsKey(state)) // Explore
        {
            return chosenAction;
        }

        var actionValues = _qTable[state];
        var qValue = 0f;
        foreach (var act in actions)
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
        float newQ = oldQ + learningRate * (reward + discountFactor * maxFutureQ - oldQ);

        // Update Q-table
        _qTable[prevState][action] = newQ;
    }

    State GetState(Tile tile)
    {
        float[] obs = new float[4];

        for (int i = 0; i < actions.Length; i++)
        {
            var adjecentTile = tile.GetAdjecentTile(actions[i]);

            if (adjecentTile != null) obs[i] = adjecentTile.GetTileTypeToInt();
            else obs[i] = -2f;
        }

        var state = new State();
        state.obs = obs;

        return state;
    }

    float GetReward(Tile tile)
    {
        return rewards[tile.GetTileType()];
    }

    void SetReward(TileType type, float value)
    {
        rewards[type] = value;
    }


    void ResetLevel()
    {
        _player.ResetAgent();

        foreach (var tile in _tilesToReset)
        {
            tile.ResetTile();
        }
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
}
