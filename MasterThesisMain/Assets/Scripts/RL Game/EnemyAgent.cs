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
        _prevTile = controller.currentTile;
        controller.isEnemy = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (!_activated) return;

        if (controller.IsDead)
        {
            return;
        }

        if (!_calculatingMove && !_player.IsMoving())
        {
            _calculatingMove = true;
            _prevTile = controller.currentTile;

            var state = GetState(controller.currentTile);

            var action = GetAction(state);

            controller.MoveToSelectedAction(action);

            controller.currentTile.SetCurrentType(TileType.Enemy);
        }

        if (!controller.IsDead && (_player.currentTile == controller.currentTile || _player.currentTile == _prevTile) && !_calculatingMove )
        {
            // Debug.Log("Died");

            if (!_vulnerable)
            {
                _player.IsDead = true;
            }
            else
            {
                controller.IsDead = true;
                HideBody();
                controller.currentTile.ResetTile();
                _prevTile.ResetTile();
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
        base.ResetAgent();
        _vulnerable = false;
        controller.HideModel(false);
        _prevTile = controller.startingTile;

    }

    void HideBody()
    {
        controller.HideModel(true);
    }
}
