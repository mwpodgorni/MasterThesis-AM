using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

// A Simple Q-Learning agent
public class DeepQAgent : RLAgent
{
    [Header("Neural Network")]
    [SerializeField] NeuralNetwork _network;
    [SerializeField] int _inputSize = 5;
    
    [SerializeField] int _hiddenSize = 8;
    [SerializeField] int _layerCount = 2;

    [SerializeField] protected Tile _goalTile;

    Tile prevTile;

    // Start is called before the first frame update
    void Start()
    {
        if (_network == null) TryGetComponent(out _network);
        else
        {
            for (int i = 0; i < _inputSize; i++)
                _network.AddInputLayerNode();

            for (int i = 0; i < _actions.Length; i++)
                _network.AddOutputLayerNode();

            for (int i = 0; i < _layerCount; i++)
            {
                _network.AddHiddenLayer();

                for (int j = 0; j < _hiddenSize; j++)
                {
                    _network.AddHiddenLayerNode(i);
                }
            }
        }

        _rewards[TileType.Normal] = 0;
        _rewards[TileType.Wall] = -1;

        prevTile = _controller.currentTile;
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
            _calculatingMove = true;

            var action = GetAction(GetState(_controller.currentTile));

            var nextTile = _controller.GetTile(action);

            if (nextTile == null)
            {
                return;
            }

            _epsilon = _epsilonMin + (1.0f - _epsilonMin) * Mathf.Exp(-_decayRate * totalStepCount);
            totalStepCount++;

            // Calculate the current distance to the goal
            float currentDistance = Vector3.Distance(_controller.currentTile.point.position, _goalTile.point.position);

            // Calculate the previous distance to the goal (stored from the previous step)
            float previousDistance = Vector3.Distance(prevTile.point.position, _goalTile.point.position);

            // Reward based on the change in distance
            float distanceReward = previousDistance - currentDistance;

            var reward = GetReward(nextTile) + distanceReward;
            totalReward += reward;

            var nextObs = GetState(nextTile);

            // Calculate Q-target (based on reward and max future Q-value)
            float maxFutureQ = _network.ForwardPass(nextObs.obs).Max();
            float qTarget = reward + _discountFactor * maxFutureQ;

            float[] currentObs = GetState(_controller.currentTile).obs;
            float[] currentQValues = _network.ForwardPass(GetState(_controller.currentTile).obs);
            currentQValues[(int)action] = qTarget;

            _network.BackPropagate(currentObs, currentQValues);

            _controller.MoveToSelectedAction(action);
            
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

        if (totalStepCount % maxSteps == 0)
        {
            _finishedEpoch = true;
        }
    }

    override public Action GetAction(State state)
    {
        var possibleActions = _controller.GetPossibleActions();

        Random.InitState(DateTime.Now.Millisecond);

        if (Random.value < _epsilon)
        {
            return possibleActions[Random.Range(0, possibleActions.Count)];
        }

        float[] qValues = _network.ForwardPass(state.obs);

        float maxQ = float.NegativeInfinity;
        Action bestAction = possibleActions[0];

        // Only consider valid actions
        foreach (var action in possibleActions)
        {
            int actionIndex = Array.IndexOf(_actions, action);
            if (actionIndex >= 0 && qValues[actionIndex] > maxQ)
            {
                maxQ = qValues[actionIndex];
                bestAction = action;
            }
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < qValues.Length; i++)
        {
            sb.Append(qValues[i] + "-" + _actions[i]);
            sb.Append(" ");
        }
        Debug.Log(sb.ToString());

        return bestAction;
    }

    public override void ResetModel()
    {
    }
}
