using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReachGoalTask : Task
{
    [SerializeField] RLAgent agent;
    [SerializeField] Tile goalTile;

    public override bool IsComplete()
    {
        return agent.controller.currentTile == goalTile;
    }
}
