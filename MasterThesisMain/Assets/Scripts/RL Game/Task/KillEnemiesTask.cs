using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillEnemiesTask : Task
{
    [SerializeField] List<EnemyAgent> enemies;

    public override bool IsComplete()
    {
        foreach (var enemy in enemies)
        {
            if (!enemy.controller.IsDead) return false;
        }

        return true;
    }
}
