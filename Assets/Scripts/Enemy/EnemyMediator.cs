using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class EnemyMediator
{
    private static List<EnemyBase> registeredEnemies = new List<EnemyBase>();
    private static List<Vector3> occupiedPatrolPoints = new List<Vector3>();
    
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
    
    
    public static Vector3 GetPatrolPoint(Vector3 origin, float range, float minDistance)
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
        int maxAttempts = 5;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int triangleIndex = Random.Range(0, navMeshData.indices.Length / 3) * 3;
            Vector3 vertex1 = navMeshData.vertices[navMeshData.indices[triangleIndex]];
            Vector3 vertex2 = navMeshData.vertices[navMeshData.indices[triangleIndex + 1]];
            Vector3 vertex3 = navMeshData.vertices[navMeshData.indices[triangleIndex + 2]];
            Vector3 randomPoint = GetRandomPointInTriangle(vertex1, vertex2, vertex3);
            //Vector3 randomPoint = origin + new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
            
            if (Vector3.Distance(origin, randomPoint) <= range && IsPointValid(randomPoint, minDistance))
            {
                occupiedPatrolPoints.Add(randomPoint);
                return randomPoint;
            }
        }
        
        return origin; 
    }
    
    private static bool IsPointValid(Vector3 point,  float minDistance)
    {
        foreach (var occupiedPoint in occupiedPatrolPoints)
        {
            if (Vector3.Distance(point, occupiedPoint) < minDistance)
            {
                return false; 
            }
        }
        return true; 
    }
    
    private static Vector3 GetRandomPointInTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        float a = Random.value;
        float b = Random.value;
        
        if (a + b > 1f)
        {
            a = 1f - a;
            b = 1f - b;
        }

        float c = 1f - a - b;
        return a * v1 + b * v2 + c * v3;
    }

    
    public static void ReleasePatrolPoint(Vector3 point)
    {
        if (occupiedPatrolPoints.Contains(point))
        {
            occupiedPatrolPoints.Remove(point);
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