using System.Collections.Generic;
using UnityEngine;
using Alexwsu.EventChannels;

public class RLManager : MonoBehaviour
{
    [Header("Level Elements")]
    [SerializeField] RLAgent _player;
    [SerializeField] List<EnemyAgent> _enemies;
    [SerializeField] List<Tile> _tilesToReset;
    [SerializeField] List<TileType> _observedTiles;

    [Header("Level Settings")]
    [SerializeField] int requiredCountOfCompletedTask = 3;
    [SerializeField] GameSpeed _speed = GameSpeed.Normal;
    [SerializeField] float _normalSpeed = 1f;
    [SerializeField] float _fastSpeed = 3f;
    [SerializeField] float _fasterSpeed = 5f;

    public int episodeCount = 1;
    public int maxEpisodes = 30;
    public int maxStepPerEpoch = 20;

    [Header("Statistics")]
    public float maxReward = 0;
    public float avgRewardPerEpoch = 0;

    bool _training = false;
    int _taskCompletedCount = 0;

    public bool LevelCompleted
    {
        get
        {
            return _taskCompletedCount >= requiredCountOfCompletedTask;
        }
    }

    [Tooltip("Invokes this event channel when training is finished")]
    [SerializeField] EventChannel _unsolvedChannel;
    [SerializeField] EventChannel _solvedChannel;

    private void Start()
    {
        SetSpeed(_speed);
        _player.maxSteps = maxStepPerEpoch;

        SetReward(TileType.Enemy, 0);
        SetReward(TileType.Goal, 0);
        SetReward(TileType.Collectible, 0);
        SetReward(TileType.Buff, 0);
    }

    private void Update()
    {
        if (_player.FinishedEpoch && _training)
        {
            if (episodeCount >= maxEpisodes || _taskCompletedCount >= requiredCountOfCompletedTask)
            {
                // Training finished
                _training = false;
                avgRewardPerEpoch = _player.avgRewardPerEpoch;
                episodeCount = 0;
                DeactivateAgents();
                ResetTraining();

                if (_taskCompletedCount < requiredCountOfCompletedTask)
                {
                    _unsolvedChannel.Invoke(new Empty());
                }
                else
                {
                    _solvedChannel.Invoke(new Empty());
                }
                
            }
            else
            {
                Debug.Log("Episode Finished");
                episodeCount++;
                ResetTraining(); // Reset for next epoch
            }
        }
    }

    public void StartTraining()
    {
        if (!_training)
        {
            _training = true;
            ResetModel();
            ResetTraining();
            ActivateAgents();
            episodeCount = 1;
        }
    }

    public void ResetTraining()
    {
        _player.ResetAgent();
        
        if (_enemies.Count > 0)
        {
            foreach (var enemy in _enemies)
            {
                enemy.ResetAgent();
            }
        }

        if (_tilesToReset.Count > 0)
        {
            foreach (var tile in _tilesToReset)
            {
                tile.ResetTile();
            }
        }
    }

    public void ResetModel()
    {
        _player.ResetModel();
    }

    public void SetSpeed(GameSpeed mode)
    {
        switch (mode)
        {
            case GameSpeed.Normal:
                Time.timeScale = _normalSpeed;
                break;
            case GameSpeed.Fast:
                Time.timeScale = _fastSpeed; 
                break;
            case GameSpeed.Faster:
                Time.timeScale = _fasterSpeed;
                break;
        }
    }

    public void SetReward(TileType type, float value)
    {
        if (_training) return;
        _player.SetReward(type, value);
    }

    public void ToggleSpeed()
    {
        switch (_speed)
        {
            case GameSpeed.Normal:
                _speed = GameSpeed.Fast;
                break;
            case GameSpeed.Fast:
                _speed = GameSpeed.Faster;
                break;
            case GameSpeed.Faster:
                _speed = GameSpeed.Normal;
                break;
        }

        SetSpeed(_speed);
    }

    public void ActivateAgents()
    {
        _player.Activated = true;
        if (_enemies.Count > 0)
        {
            foreach (var enemy in _enemies)
            {
                enemy.Activated = true;
            }
        }
    }

    public void DeactivateAgents()
    {
        _player.Activated = false;
        if (_enemies.Count > 0)
        {
            foreach (var enemy in _enemies)
            {
                enemy.Activated = false;
            }
        }
    }

    public void IncreaseSolvedCount()
    {
        _taskCompletedCount++;
    }

    public List<TileType> GetObservedTiles()
    {
        return _observedTiles;
    }

    public enum GameSpeed
    {
        Normal,
        Fast, 
        Faster
    }
}

public struct RLEvaluationData
{
    public float learningRate;   // Alpha (α)
    public float discountFactor; // Gamma (γ);
    public float decayRate;
    public float epsilonMin;
    public int maxSteps;

    public float avgEpisodeReward; // total reward avg
    public float avgEpisodeLength; // number of steps 
    public float successRate; // rate of how successful it completes tasks
}
