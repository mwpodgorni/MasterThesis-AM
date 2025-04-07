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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_controller.IsDead())
        {
            _hideBody = true;
        }

        if (!_calculatingMove && !_player.IsMoving())
        {
            _calculatingMove = true;

            _controller.MoveToSelectedAction(GetAction(_controller.currentTile));
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
    }

    Action GetAction(Tile tile)
    {
        var possibleActions = _player.GetPossibleActions();

        // Choose action using ε-greedy strategy
        Random.InitState(DateTime.Now.Millisecond);

        return possibleActions[Random.Range(0, possibleActions.Count)];
    }

    void HideBody()
    {
        _hideBody = true;
    }
}
