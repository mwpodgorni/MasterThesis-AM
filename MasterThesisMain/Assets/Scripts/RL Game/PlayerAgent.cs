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

    [SerializeField] InputReader _inputs;

    // Start is called before the first frame update
    void Start()
    {
        if (!gameObject.TryGetComponent<NavMeshAgent>(out agent))
        {
            Debug.LogWarning("Error: Player Agent does not have a NavMesh Agent Component");
        }

        if (currentTile == null)
        {
            Debug.LogWarning("Error: No starting tile set.");
        }
    }

    // Update is called once per frame
    void Update()
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

    void MoveToTile(Tile tile)
    {
        if (!_moving)
        {
            if (tile.GetTileType() == TileType.Wall) return;

            agent.SetDestination(tile.point.position);
            currentTile = tile;
            _moving = true;
            _didAction = true;
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
