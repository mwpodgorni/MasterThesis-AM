using System.Collections.Generic;
using UnityEngine;
using Alexwsu.EventChannels;
using JetBrains.Annotations;
using UnityEngine.UIElements;

public class RLManager : MonoBehaviour
{
    [Header("Level Elements")]
    [SerializeField] RLAgent _player;
    [SerializeField] List<EnemyAgent> _enemies;
    [SerializeField] List<Tile> _tilesToReset;
    [SerializeField] List<TileType> _observedTiles;

    [Header("Level Settings")]
    [SerializeField] GameSpeed _speed = GameSpeed.Normal;
    [SerializeField] float _normalSpeed = 1f;
    [SerializeField] float _fastSpeed = 3f;
    [SerializeField] float _fasterSpeed = 5f;
    [SerializeField] float _fastestSpeed = 8f;

    [Header("Agent Settings")]
    public int requiredCountOfCompletedTask = 3;
    public int maxEpisodes = 30;
    public int maxStepPerEpoch = 20;
    public float epsilonDecayRate = 0.01f;
    public float learningRate = 0.1f;

    [SerializeField] List<RLSettings> _rlSettings;

    [Header("Statistics")]
    public float maxReward = 0;
    public float avgRewardPerEpoch = 0;

    bool _training = false;
    int _taskCompletedCount = 0;

    public int episodeCount = 1; // How many episodes the agent has been through since the last reward change.

    public float[] episodeReward; // Shows if the agent is earning more reward.
    public float[] successRateRolling; // Rolling average success rate over the last N episodes
    public float[] stepsToCompletion; // To indicate whether the agent is becoming more efficient

    public RLEvaluationData currentEval;
    ProgressBar _progressBar;
    public bool LevelCompleted
    {
        get
        {
            return _taskCompletedCount >= requiredCountOfCompletedTask;
        }
    }

    [Tooltip("Invokes this event channel when training is finished")]
    [SerializeField] BoolEventChannel _unsolvedChannel;

    private void Start()
    {
        SetSpeed(_speed);

        episodeReward = new float[maxEpisodes];
        successRateRolling = new float[maxEpisodes];
        stepsToCompletion = new float[maxEpisodes];

        SetReward(TileType.Enemy, 0);
        SetReward(TileType.Goal, 0);
        SetReward(TileType.Collectible, 0);
        SetReward(TileType.Buff, 0);

        currentEval = new RLEvaluationData();

        var uiDoc = Object.FindFirstObjectByType<UIDocument>();
        var ui = uiDoc.rootVisualElement;
        _progressBar = ui.Q<ProgressBar>("ProgressBar");
        _progressBar.lowValue = 0;
        _progressBar.highValue = maxEpisodes;
        _progressBar.value = episodeCount;
    }

    private void Update()
    {
        if (_player.FinishedEpoch && _training)
        {
            if (episodeCount >= maxEpisodes)
            {
                // Training finished
                UpdateEval();

                _unsolvedChannel.Invoke(_taskCompletedCount >= requiredCountOfCompletedTask);
                _progressBar.value = episodeCount;
                StopTraining();
            }
            else
            {
                Debug.Log("Episode Finished");
                episodeCount++;

                episodeReward[episodeCount - 1] = _player.currentEpochReward;
                successRateRolling[episodeCount - 1] = _player.totalTaskCompleted / (float)episodeCount;
                stepsToCompletion[episodeCount - 1] = (float)_player.currentEpochStepCount / (float)maxStepPerEpoch;
                _progressBar.value = episodeCount;
                ResetTraining(); // Reset for next epoch
                UpdateEval();
                RLController.Instance.UpdateEvaluation();
            }
        }
    }

    public void SetTraining()
    {
        _player.maxSteps = maxStepPerEpoch;
        _player.decayRate = epsilonDecayRate;
        _player.learningRate = learningRate;
    }

    public void StartTraining()
    {
        if (_training) return;

        SetTraining();
        _training = true;
        ResetModel();
        ResetTraining();
        ActivateAgents();
        episodeCount = 1;
    }

    public void StopTraining()
    {
        if (!_training) return;

        _training = false;
        DeactivateAgents();
        ResetModel();
        ResetTraining();
        episodeCount = 1;

        SetSpeed(GameSpeed.Normal);
        _speed = GameSpeed.Normal;
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
        _taskCompletedCount = 0;
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
            case GameSpeed.Fastest:
                Time.timeScale = _fastestSpeed;
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
                _speed = GameSpeed.Fastest;
                break;
            case GameSpeed.Fastest:
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

    public List<RLSettings> GetRLSettings()
    {
        return _rlSettings;
    }

    public void UpdateEval()
    {
        currentEval.episodeReward = episodeReward;
        currentEval.successRateRolling = successRateRolling;
        currentEval.stepsToCompletion = stepsToCompletion;

        currentEval.avgEpisodeReturn = _player.totalReward / maxEpisodes;
        currentEval.successRate = _player.totalTaskCompleted / maxEpisodes;
        currentEval.completionTime = _player.totalStepCount / (float)(maxEpisodes);
        currentEval.episodeCount = episodeCount;
    }

}

public enum RLSettings
{
    LearningRate,
    DecayRate,
    Steps,
    Episodes
}

public enum GameSpeed
{
    Normal,
    Fast,
    Faster,
    Fastest
}
public struct RLEvaluationData
{
    public float avgEpisodeReturn; // total accumulated reward in a single episode.
    public float successRate; // Percentage of episodes where the agent successfully completed the task.
    public float completionTime; // How long it takes the agent to complete the task (if it does).
    public float episodeCount; // How many episodes the agent has been through since the last reward change.

    public float[] episodeReward; // Shows if the agent is earning more reward.
    public float[] successRateRolling; // Rolling average success rate over the last N episodes
    public float[] stepsToCompletion; // To indicate whether the agent is becoming more efficient
}
