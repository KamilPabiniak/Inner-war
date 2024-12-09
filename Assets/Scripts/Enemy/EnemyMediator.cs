using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class EnemyMediator
{
    private static List<EnemyBase> registeredEnemies = new List<EnemyBase>();
    private static List<Vector3> occupiedPatrolPoints = new List<Vector3>();
    private static float MinPatrolPointDistance = 5f;

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
    
    
    public static Vector3 GetPatrolPoint(Vector3 origin, float range)
    {
        int maxAttempts = 5;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPoint = origin + new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
            
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, range, NavMesh.AllAreas))
            {
                Vector3 validPoint = hit.position;
                
                if (IsPointValid(validPoint))
                {
                    occupiedPatrolPoints.Add(validPoint);
                    return validPoint;
                }
            }
        }

        Debug.LogError($"Nie znaleziono odpowiedniego punktu patrolowego po {maxAttempts} próbach.");
        return origin; 
    }
    
    private static bool IsPointValid(Vector3 point)
    {
        foreach (var occupiedPoint in occupiedPatrolPoints)
        {
            if (Vector3.Distance(point, occupiedPoint) < MinPatrolPointDistance)
            {
                return false; 
            }
        }
        return true; 
    }

    
    public static void ReleasePatrolPoint(Vector3 point)
    {
        if (occupiedPatrolPoints.Contains(point))
        {
            occupiedPatrolPoints.Remove(point);
        }
        else
        {
            Debug.LogWarning($"Próba zwolnienia punktu, który nie jest zajêty: {point}");
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