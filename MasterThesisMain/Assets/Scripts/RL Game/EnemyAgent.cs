using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAgent : RLAgent
{
    [SerializeField] AgentController _player;

    bool _vulnerable = false;

    void Start()
    {
        _player = FindObjectOfType<QAgent>()?.controller;
        _prevTile = controller.currentTile;
        _currentTile = controller.currentTile;
        controller.isEnemy = true;
    }

    void Update()
    {
        if (!_activated) return;

        if (controller.IsDead)
        {
            return;
        }

        if (!_calculatingMove && !controller.IsMoving())
        {
            _calculatingMove = true;
            _currentTile = controller.currentTile;

            var state = GetState(controller.currentTile);

            var action = GetAction(state);

            _prevTile = controller.currentTile;
            controller.MoveToSelectedAction(action);

            controller.currentTile.SetCurrentType(TileType.Enemy);
        }

        if (!controller.IsDead && (_player.currentTile == controller.currentTile || _player.currentTile == _prevTile) && !_calculatingMove)
        {
            if (!_vulnerable)
            {
                _player.IsDead = true;
            }
            else
            {
                controller.IsDead = true;
                HideBody();

                if (_prevTile.isUsed) _currentTile.SoftReset();
                else _currentTile.ResetTile();

                if (_prevTile.isUsed) _prevTile.SoftReset();
                else _prevTile.ResetTile();
            }
        }

        if (_calculatingMove && _timer < _waitTime)
        {
            _timer += Time.deltaTime;
        }
        else
        {
            if (_prevTile.isUsed) _prevTile.SoftReset();
            else _prevTile.ResetTile();
            _calculatingMove = false;
            _timer = 0f;
        }
    }

    override public Action GetAction(State state)
    {
        var possibleActions = controller.GetPossibleActions();

        Random.InitState(DateTime.Now.Millisecond);

        return possibleActions[Random.Range(0, possibleActions.Count)];
    }

    public void SetVulnerable(bool value)
    {
        _vulnerable = value;
    }

    override public void ResetAgent()
    {
        _vulnerable = false;
        controller.HideModel(false);

        base.ResetAgent();
    }

    void HideBody()
    {
        controller.HideModel(true);
    }
}
