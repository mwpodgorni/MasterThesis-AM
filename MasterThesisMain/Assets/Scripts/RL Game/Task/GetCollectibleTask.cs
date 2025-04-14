using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCollectibleTask : Task
{
    [SerializeField] List<Tile> collectibleTiles;
    public override bool IsComplete()
    {
        foreach (var tile in collectibleTiles)
        {
            if (!tile.isUsed) return false;
        }

        return true;
    }
}
