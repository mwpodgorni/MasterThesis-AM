using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAgent : RLAgent
{
    [SerializeField] AgentController _player;

    [SerializeField] bool _randomMovement = true;

    bool _vulnerable = false;
    Tile _prevTile;

    // Start is called before the first frame update
    void Start()
    {
        _prevTile = _controller.currentTile;
        _controller.isEnemy = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (!_activated) return;

        if (_controller.IsDead)
        {
            return;
        }

        if (!_calculatingMove && !_player.IsMoving())
        {
            _calculatingMove = true;
            _prevTile = _controller.currentTile;

            var state = GetState(_controller.currentTile);

            var action = GetAction(state);

            _controller.MoveToSelectedAction(action);

            _controller.currentTile.SetCurrentType(TileType.Enemy);
        }

        if (!_controller.IsDead && (_player.currentTile == _controller.currentTile || _player.currentTile == _prevTile) && !_calculatingMove )
        {
            Debug.Log("Died");

            if (!_vulnerable)
            {
                _player.IsDead = true;
            }
            else
            {
                _controller.IsDead = true;
                HideBody();
                _controller.currentTile.ResetTile();
                _prevTile.ResetTile();
            }
        }

        if (_calculatingMove && _timer < _waitTime)
        {
            _timer += Time.deltaTime;
        }
        else
        {
            _prevTile.ResetTile();
            _calculatingMove = false;
            _timer = 0f;
        }
    }

    override public Action GetAction(State state)
    {
        var possibleActions = _controller.GetPossibleActions();

        Random.InitState(DateTime.Now.Millisecond);

        return possibleActions[Random.Range(0, possibleActions.Count)];
    }

    public void SetVulnerable(bool value)
    {
        _vulnerable = value;
    }

    override public void ResetAgent()
    {
        base.ResetAgent();
        _vulnerable = false;
        _controller.HideModel(false);
        _prevTile = _controller.startingTile;

    }

    void HideBody()
    {
        _controller.HideModel(true);
    }
}
