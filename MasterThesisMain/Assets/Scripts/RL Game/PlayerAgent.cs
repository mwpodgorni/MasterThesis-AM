using Alexwsu.EventChannels;
using DG.Tweening;
using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class PlayerAgent : MonoBehaviour
{
    [Header("Properties")]
    public Tile currentTile;
    public Tile startingTile;

    [SerializeField] Transform _model;

    [Header("External Components")]
    [SerializeField] NavMeshAgent _agent;
    [SerializeField] InputReader _inputs;
    [SerializeField] LineRenderer _line;

    bool _moving;
    bool _didAction;
    float _inputWaitingtime = 1f;
    float _waitingTime = 0.0f;
    bool _dead = false;
    int _tileIndex = 0;

    Action _prevAction;

    // Start is called before the first frame update
    void Start()
    {
        if (!gameObject.TryGetComponent<NavMeshAgent>(out _agent))
        {
            Debug.LogWarning("Error: Player Agent does not have a NavMesh Agent Component");
        }

        if (!gameObject.TryGetComponent<LineRenderer>(out _line))
        {
            Debug.LogWarning("Error: Player Agent does not have a Line Renderer Component");
        }

        if (startingTile == null)
        {
            Debug.LogWarning("Error: No starting tile set.");
        }
        else
        {
            currentTile = startingTile;
            transform.position = startingTile.point.position;
            _line.SetPosition(0, startingTile.point.position);
        }

        _model.DOMoveY(1f, 0.75f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_dead)
        {
            if (_waitingTime < _inputWaitingtime && _didAction)
            {
                _waitingTime += Time.deltaTime;
                return;
            }
            else
            {
                _waitingTime = 0.0f;
                _didAction = false;
            }

            if (currentTile.GetTileType() == TileType.Dangerous || currentTile.GetTileType() == TileType.Goal)
            {
                _dead = true;
                return;
            }

            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                _moving = false;
            }

            if (!_moving)
            {
                DoAction();
            }
        }
    }

    void MoveToTile(Tile tile)
    {
        if (!_moving)
        {
            if (tile.GetTileType() == TileType.Wall) return;

            _agent.SetDestination(tile.point.position);
            currentTile = tile;
            _moving = true;
            _didAction = true;

            _tileIndex++;
            _line.positionCount = _tileIndex + 1;
            _line.SetPosition(_tileIndex, tile.point.position);

            if (tile.GetTileType() == TileType.Collectible)
            {
                tile.Use();
            }

        }
        
    }

    public Tile GetTile(Action action)
    {
        return currentTile.GetAdjecentTile(action);
    }

    public Tile MoveToSelectedAction(Action action)
    {
        if (_moving) return null;

        var tile = GetTile(action);

        if (tile == null) return null;

        MoveToTile(tile);
        return tile;
    }

    public bool IsMoving() { return _moving; }
    public bool IsDead() { return _dead; }

    void DoAction()
    {
        if (_inputs.Movement.y > 0) MoveToSelectedAction(Action.Up);
        if (_inputs.Movement.y < 0) MoveToSelectedAction(Action.Down);
        if (_inputs.Movement.x > 0) MoveToSelectedAction(Action.Right);
        if (_inputs.Movement.x < 0) MoveToSelectedAction(Action.Left);
    }

    public void ResetAgent()
    {
        currentTile = startingTile;
        transform.position = startingTile.point.position;
        _agent.SetDestination(startingTile.point.position);

        _line.positionCount = 1;
        _tileIndex = 0;

        _dead = false;
        _moving = false;
    }

}
