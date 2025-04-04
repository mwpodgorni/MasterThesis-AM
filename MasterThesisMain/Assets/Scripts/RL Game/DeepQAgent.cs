using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

// A Simple Q-Learning agent
public class DeepQAgent : MonoBehaviour
{
    [Header("Deep-Q Learning Parameters")]
    // Q-Table
    [SerializeField] NeuralNetwork _network;

    // Possible Actions
    Action[] actions = {Action.Up, Action.Down, Action.Left, Action.Right};

    public float learningRate = 0.1f;
    public float discountFactor = 0.9f;
    public float decayRate = 0.01f;
    
    public float epsilon = 0f;
    float epsilonMin = 0.01f;
    
    public int totalStepCount = 0;
    public int currentEpisodeSteps = 0;
    public int episodeCount = 0;
    public int maxSteps = 20;

    Dictionary<TileType, float> rewards = new Dictionary<TileType, float>();

    [Header("Properties")]
    [SerializeField] Tile _goalTile;
    [SerializeField] PlayerAgent _player;
    [SerializeField] Tile[] _tilesToReset;

    bool _calculatingMove = false;
    float _waitTime = 1f;
    float _timer = 0f;
    Tile prevTile;

    // Start is called before the first frame update
    void Start()
    {
        if (_network == null) TryGetComponent<NeuralNetwork>(out _network);

        rewards[TileType.Normal] = 0;
        rewards[TileType.Dangerous] = -1;
        rewards[TileType.Wall] = -1;
        rewards[TileType.Collectible] = 0.5f;
        rewards[TileType.Goal] = 1f;

        prevTile = _player.currentTile;
    }

    // Update is called once per frame
    void Update()
    {
        if(!_calculatingMove && !_player.IsMoving())
        {
            _calculatingMove = true;

            var action = ChooseAction(_player.currentTile);

            var nextTile = _player.GetTile(action);

            if (nextTile == null)
            {
                return;
            }

            epsilon = epsilonMin + (1.0f - epsilonMin) * Mathf.Exp(-decayRate * totalStepCount);
            totalStepCount++;
            currentEpisodeSteps++;

            // Calculate the current distance to the goal
            float currentDistance = Vector3.Distance(_player.currentTile.point.position, _goalTile.point.position);

            // Calculate the previous distance to the goal (stored from the previous step)
            float previousDistance = Vector3.Distance(prevTile.point.position, _goalTile.point.position);

            // Reward based on the change in distance
            float distanceReward = previousDistance - currentDistance;

            var reward = GetRewardByTileType(nextTile.GetTileType()) + distanceReward;

            var nextObs = GetTileObservations(nextTile);

            // Calculate Q-target (based on reward and max future Q-value)
            float maxFutureQ = _network.ForwardPass(nextObs).Max();
            float qTarget = reward + discountFactor * maxFutureQ;

            float[] currentObs = GetTileObservations(_player.currentTile);
            float[] currentQValues = _network.ForwardPass(GetTileObservations(_player.currentTile));
            currentQValues[(int)action] = qTarget;

            _network.BackPropagate(currentObs, currentQValues);

            _player.MoveToSelectedAction(action);
            
        }

        if(_calculatingMove && _timer < _waitTime)
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

    float[] GetTileObservations(Tile tile)
    {
        float [] observation = new float[_network.inputLayer.nodes.Count];

        for (int i = 0; i < actions.Length; i++)
        {
            var adjecentTile = tile.GetAdjecentTile(actions[i]);

            if (adjecentTile != null) observation[i] = GetRewardByTileType(adjecentTile.GetTileType());
            else observation[i] = -2f;
        }

        observation[4] = Vector3.Distance(_player.currentTile.point.position, _goalTile.point.position);

        return observation;
    }


    Action ChooseAction(Tile tile)
    {
        var possibleActions = _player.GetPossibleActions();

        Random.InitState(DateTime.Now.Millisecond);

        if (Random.value < epsilon)
        {
            return possibleActions[Random.Range(0, possibleActions.Count)];
        }

        float[] qValues = _network.ForwardPass(GetTileObservations(tile));

        float maxQ = float.NegativeInfinity;
        Action bestAction = possibleActions[0];

        // Only consider valid actions
        foreach (var action in possibleActions)
        {
            int actionIndex = Array.IndexOf(actions, action);
            if (actionIndex >= 0 && qValues[actionIndex] > maxQ)
            {
                maxQ = qValues[actionIndex];
                bestAction = action;
            }
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < qValues.Length; i++)
        {
            sb.Append(qValues[i] + "-" + actions[i]);
            sb.Append(" ");
        }
        Debug.Log(sb.ToString());

        return bestAction;
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
