using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Tile Type")]
    [SerializeField] TileType _type = TileType.Normal;

    [Header("Properties")]
    public Transform point;
    public GameObject model;

    [Header("Adjecent Tiles")]
    public Dictionary<Action, Tile> adjecentTiles = new Dictionary<Action, Tile>();

    public void Start()
    {
        SetAdjecentTiles();
    }

    public TileType GetTileType()
    {
        return _type;
    }

    public int GetTileTypeToInt()
    {
        return (int) _type;
    }

    public void Use()
    {
        model.SetActive(false);
        _type = TileType.Normal;
    }

    void SetAdjecentTiles()
    {
        if (Physics.Raycast(transform.position, Vector3.right, out RaycastHit right, 4))
            if (right.collider.gameObject.TryGetComponent<Tile>(out Tile tile)) 
                adjecentTiles[Action.Right] = tile;

        if (Physics.Raycast(transform.position, Vector3.left, out RaycastHit left, 4))
            if (left.collider.gameObject.TryGetComponent<Tile>(out Tile tile))
                adjecentTiles[Action.Left] = tile;

        if (Physics.Raycast(transform.position, Vector3.forward, out RaycastHit up, 4))
            if (up.collider.gameObject.TryGetComponent<Tile>(out Tile tile))
                adjecentTiles[Action.Up] = tile;

        if (Physics.Raycast(transform.position, Vector3.back, out RaycastHit down, 4))
            if (down.collider.gameObject.TryGetComponent<Tile>(out Tile tile))
                adjecentTiles[Action.Down] = tile;
    }

}

public enum TileType
{
    Normal = 0,
    Wall = 1,
    Dangerous = 2,
    Collectible = 3,
    Goal = 4
}
