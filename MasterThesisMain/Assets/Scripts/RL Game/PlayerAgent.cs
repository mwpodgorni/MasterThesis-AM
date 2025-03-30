using Alexwsu.EventChannels;
using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class PlayerAgent : MonoBehaviour
{
    public Tile currentTile;
    public NavMeshAgent agent;

    [SerializeField] Collider _collider;
    [SerializeField] IntEventChannel _eventChannel;

    bool _moving;
    bool _didAction;
    float _inputWaitingtime = 1f;
    float _waitingTime = 0.0f;
    bool _dead = false;

    int _tileIndex = 0;

    Action _prevAction;

    [SerializeField] InputReader _inputs;
    [SerializeField] LineRenderer _line;

    // Start is called before the first frame update
    void Start()
    {
        if (!gameObject.TryGetComponent<NavMeshAgent>(out agent))
        {
            Debug.LogWarning("Error: Player Agent does not have a NavMesh Agent Component");
        }

        if (!gameObject.TryGetComponent<LineRenderer>(out _line))
        {
            Debug.LogWarning("Error: Player Agent does not have a Line Renderer Component");
        }

        if (currentTile == null)
        {
            Debug.LogWarning("Error: No starting tile set.");
        }
        else
        {
            _line.SetPosition(0, currentTile.point.position);
        }

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

            if (agent.remainingDistance <= agent.stoppingDistance)
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

            agent.SetDestination(tile.point.position);
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

            if (tile.GetTileType() == TileType.Dangerous) _dead = true;

        }
        
    }

    public Tile GetTile(Action action)
    {
        var tile = currentTile.adjecentTiles[action];
        return tile;
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

    void DoAction()
    {
        if (_inputs.Movement.y > 0) MoveToSelectedAction(Action.Up);
        if (_inputs.Movement.y < 0) MoveToSelectedAction(Action.Down);
        if (_inputs.Movement.x > 0) MoveToSelectedAction(Action.Right);
        if (_inputs.Movement.x < 0) MoveToSelectedAction(Action.Left);
    }

}
