using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// A Simple Q-Learning agent
public class RLAgent : MonoBehaviour
{
    [Header("RL Parameters")]
    // Q-Table
    [SerializeField] NeuralNetwork _network;

    // Possible Actions
    Action[] actions = {Action.Up, Action.Down, Action.Left, Action.Right};

    public float learningRate = 0.1f;
    public float discountFactor = 0.9f;
    public float decayRate = 0.01f;
    
    public float epsilon = 1f;
    float epsilonMin = 0.01f;
    
    public int stepCount = 0;
    public int episodeCount = 0;

    Dictionary<TileType, float> rewards = new Dictionary<TileType, float>();

    [Header("Properties")]
    [SerializeField] PlayerAgent _player;
    [SerializeField] Tile[] _tilesToReset;

    // Start is called before the first frame update
    void Start()
    {
        rewards[TileType.Normal] = 0;
        rewards[TileType.Dangerous] = -1;
        rewards[TileType.Wall] = -1;
        rewards[TileType.Collectible] = 0.5f;
        rewards[TileType.Goal] = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if(!_player.IsMoving())
        {
            var action = ChooseAction(_player.currentTile);

            var nextTile = _player.GetTile(action);

            if (nextTile == null)
            {
                return;
            }

            epsilon = epsilonMin + (1.0f - epsilonMin) * Mathf.Exp(-decayRate * stepCount);
            stepCount++;

            var reward = GetRewardByTileType(nextTile.GetTileType());

            var nextObs = GetTileObservations(nextTile);

            // Calculate Q-target (based on reward and max future Q-value)
            float maxFutureQ = _network.ForwardPass(nextObs).Max();
            float qTarget = reward + discountFactor * maxFutureQ;

            float[] currentQValues = _network.ForwardPass(GetTileObservations(_player.currentTile));
            currentQValues[(int)action] = qTarget;

            _network.BackPropagate(nextObs, currentQValues);

            _player.MoveToSelectedAction(action);
        }

        if(_player.IsDead())
        {
            ResetLevel();
            episodeCount++;
        }

    }

    float[] GetTileObservations(Tile tile)
    {
        float [] observation = new float[4];

        for (int i = 0; i < actions.Length; i++)
        {   
            var adjecentTile = tile.GetAdjecentTile(actions[i]);

            if (adjecentTile != null) observation[i] = adjecentTile.GetTileTypeToInt();
            else observation[i] = GetRewardByTileType(TileType.Wall);
        }

        return observation;
    }


    Action ChooseAction(Tile tile)
    {
        var possibleActions = GetPossibleActions(tile);

        if (Random.value < epsilon)
        {
            return possibleActions[Random.Range(0,possibleActions.Count)]; // Random action (exploration)
        }

        float[] qValues = _network.ForwardPass(GetTileObservations(_player.currentTile));

        float maxQ = 0;
        int index = 0;

        for (int i = 0; i < qValues.Length; i++)
        {
            if (qValues[i] > maxQ)
            {
                maxQ = qValues[i];
                index = i;
            }
        }

        Action bestAction = actions[index];

        return bestAction;
    }

    List<Action> GetPossibleActions(Tile tile)
    {
        var possibleActions = new List<Action>();

        foreach (var act in actions)
        {
            if (tile.HasTile(act)) possibleActions.Add(act);
        }

        return possibleActions;
    }

    float GetRewardByTileType(TileType type)
    {
        return rewards[type];
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

    public void CompleteReset()
    {
        _network.RemoveWeights();
        _network.CreateWeights();

        ResetLevel();
    }

    public void SetGoalReward(float value)        { SetReward(TileType.Goal, value); }
    public void SetCollectibleReward(float value) { SetReward(TileType.Collectible, value); }
    public void SetDangerReward(float value)      { SetReward(TileType.Dangerous, value); }
}

public enum Action
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
}
