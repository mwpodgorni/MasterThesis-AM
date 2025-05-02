using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Alexwsu.EventChannels;

public class Tile : MonoBehaviour
{
    [Header("Tile Type")]
    [SerializeField] TileType _type = TileType.Normal;

    [Header("Properties")]
    public Transform point;
    public GameObject model;

    Dictionary<Action, Tile> adjecentTiles = new Dictionary<Action, Tile>();
    TileType _currentType;
    bool _used = false;

    public bool isUsed
    {
        get { return _used; }
    }

    [SerializeField] BoolEventChannel _channel;

    public void Start()
    {
        SetAdjecentTiles();
        _currentType = _type;
    }

    public TileType GetTileType()
    {
        return _currentType;
    }

    public int GetTileTypeToInt()
    {
        return (int) _currentType;
    }

    public void Use()
    {
        model.SetActive(false);
        _currentType = TileType.Normal;
        _used = true;

        if (_channel != null)
        {
            InvokeTrue();
        }
    }

    public void InvokeTrue()
    {
        _channel.Invoke(true);
    }

    public void InvokeFalse()
    {
        _channel.Invoke(false);
    }

    void SetAdjecentTiles()
    {
        if (Physics.Raycast(transform.position, Vector3.right, out RaycastHit right, 2))
            if (right.collider.gameObject.TryGetComponent(out Tile tile)) 
                adjecentTiles[Action.Right] = tile;

        if (Physics.Raycast(transform.position, Vector3.left, out RaycastHit left, 2))
            if (left.collider.gameObject.TryGetComponent(out Tile tile))
                adjecentTiles[Action.Left] = tile;

        if (Physics.Raycast(transform.position, Vector3.back, out RaycastHit up, 2))
            if (up.collider.gameObject.TryGetComponent(out Tile tile))
                adjecentTiles[Action.Up] = tile;

        if (Physics.Raycast(transform.position, Vector3.forward, out RaycastHit down, 2))
            if (down.collider.gameObject.TryGetComponent(out Tile tile))
                adjecentTiles[Action.Down] = tile;
    }

    public Tile GetAdjecentTile(Action act)
    {
        if (!adjecentTiles.TryGetValue(act, out Tile tile))
            return null;
            
        return tile;
    }

    public bool HasTile(Action act)
    {
        return adjecentTiles.ContainsKey(act);
    }

    public void ResetTile()
    {
        _currentType = _type;
        _used = false;
        if (model != null) model.SetActive(true);
    }

    public void SoftReset()
    {
        _currentType = TileType.Normal;
    }

    public void SetCurrentType(TileType type)
    {
        _currentType = type;
    }
}

public enum TileType
{
    Normal      = 0,
    Wall        = 1,
    Dangerous   = 2,
    Collectible = 3,
    Goal        = 4,
    Enemy       = 5,
    Buff        = 6,
}
