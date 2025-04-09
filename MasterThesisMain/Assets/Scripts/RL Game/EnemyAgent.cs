using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAgent : RLAgent
{
    [SerializeField] AgentController _player;

    [SerializeField] bool _randomMovement = true;
    [SerializeField] bool _hideBody = false;

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
            _hideBody = true;
            return;
        }

        if (!_calculatingMove && !_player.IsMoving())
        {
            _calculatingMove = true;

            var state = GetState(_controller.currentTile);

            var action = GetAction(state);

            _controller.MoveToSelectedAction(action);

            _prevTile.ResetTile();
            _prevTile = _controller.currentTile;
            _controller.currentTile.SetCurrentType(TileType.Enemy);
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

        if (!_controller.IsDead && (_player.currentTile == _controller.currentTile))
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
            }
        }
    }

    override public Action GetAction(State state)
    {
        var possibleActions = _controller.GetPossibleActions();

        Random.InitState(DateTime.Now.Millisecond);

        Debug.Log(possibleActions[0]);

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
    }

    void HideBody()
    {
        _controller.HideModel(true);
        _hideBody = true;
    }
}
