using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public float explorationRate = 0.2f;

    Dictionary<TileType, float> rewards = new Dictionary<TileType, float>();

    [Header("Properties")]
    [SerializeField] PlayerAgent _player;

    // Start is called before the first frame update
    void Start()
    {
        rewards[TileType.Normal] = 0;
        rewards[TileType.Dangerous] = -1;
        rewards[TileType.Wall] = -1;
        rewards[TileType.Collectible] = 0.5f;
        rewards[TileType.Goal] = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(!_player.IsMoving())
        {
            var action = ChooseAction(_player.currentTile);

            var nextTile = _player.GetTile(action);

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

    }

    float[] GetTileObservations(Tile tile)
    {
        float [] observation = new float[4];

        for (int i = 0; i < actions.Length; i++)
        {              
            if (tile.adjecentTiles.TryGetValue(actions[i], out Tile adjecentTile))
                observation[i] = tile.GetTileTypeToInt();
            else
            {
                observation[i] = GetRewardByTileType(TileType.Wall);
                continue;
            }
        }

        return observation;
    }


    Action ChooseAction(Tile tile)
    {
        var possibleActions = GetPossibleActions(tile);

        if (Random.value < explorationRate)
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
            if (!tile.adjecentTiles.TryGetValue(act, out Tile nextTile))
                continue;

            possibleActions.Add(act);
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
}



public enum Action
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
}
