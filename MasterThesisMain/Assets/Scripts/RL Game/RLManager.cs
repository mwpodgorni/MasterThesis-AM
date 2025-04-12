using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public int episodeCount = 1;
    public int maxEpisodes = 30;
    public int maxStepPerEpoch = 20;

    [Header("Statistics")]
    public float maxReward = 0;
    public float avgRewardPerEpoch = 0;

    bool _training = false;

    private void Start()
    {
        SetSpeed(_speed);
        _player.maxSteps = maxStepPerEpoch;
        
    }

    private void Update()
    {
        if (_player.FinishedEpoch && _training)
        {
            if (episodeCount >= maxEpisodes)
            {
                // Training finished
                _training = false;
                avgRewardPerEpoch = _player.avgRewardPerEpoch;
                DeactivateAgents();
                ResetTraining();
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
