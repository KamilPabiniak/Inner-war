using System.Collections.Generic;
using Enemy;
using UnityEngine;
using UnityEngine.AI;

public static class EnemyPatrolHandler
{
    private static readonly List<EnemyBase> RegisteredEnemies = new();
    private static readonly List<Vector3> OccupiedPatrolPoints = new();
    
    public static void RegisterEnemy(EnemyBase enemy)
    {
        if (!RegisteredEnemies.Contains(enemy))
        {
            RegisteredEnemies.Add(enemy);
        }
    }

    public static void UnregisterEnemy(EnemyBase enemy)
    {
        if (RegisteredEnemies.Contains(enemy))
        {
            RegisteredEnemies.Remove(enemy);
        }
    }
    
    public static Vector3 GetPatrolPoint(EnemyBase enemy)
    {
        return GetPatrolPoint(enemy.transform.position, enemy.PatrolRange, enemy.MinPatrolPointDistance);
    }

    private static Vector3 GetPatrolPoint(Vector3 origin, float range, float minDistance)
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

            if (Vector3.Distance(origin, randomPoint) <= range && IsPointValid(randomPoint, minDistance))
            {
                OccupiedPatrolPoints.Add(randomPoint);
                return randomPoint;
            }
        }
        return origin;
    }

    public static bool IsPointValid(Vector3 point, float minDistance)
    {
        foreach (var occupiedPoint in OccupiedPatrolPoints)
        {
            if (Vector3.Distance(point, occupiedPoint) < minDistance)
                return false;
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
        if (OccupiedPatrolPoints.Contains(point))
        {
            OccupiedPatrolPoints.Remove(point);
        }
    }
}
