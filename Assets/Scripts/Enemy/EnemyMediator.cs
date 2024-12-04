using System.Collections.Generic;
using UnityEngine;

public static class EnemyMediator
{
    private static List<EnemyBase> registeredEnemies = new List<EnemyBase>();

    public static void RegisterEnemy(EnemyBase enemy)
    {
        if (!registeredEnemies.Contains(enemy))
        {
            registeredEnemies.Add(enemy);
        }
    }

    public static void UnregisterEnemy(EnemyBase enemy)
    {
        if (registeredEnemies.Contains(enemy))
        {
            registeredEnemies.Remove(enemy);
        }
    }

    public static void SendAlert(Vector3 alertPosition)
    {
        Debug.Log($"Alarm! Wrogowie informowani o pozycji: {alertPosition}.");
        foreach (var enemy in registeredEnemies)
        {
            enemy.OnAlertReceived(alertPosition);
        }
    }
}