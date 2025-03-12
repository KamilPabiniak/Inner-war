using System.Collections.Generic;
using Enemy;
using UnityEngine;
using UnityEngine.AI;

public static class EnemyPatrolHandler
{
    private static readonly List<EnemyBase> RegisteredEnemies = new();
    private static readonly List<Vector3> OccupiedPatrolPoints = new();

    // Klucz: enemy, wartoœæ: wyznaczona PatrolZone
    private static readonly Dictionary<EnemyBase, SpecifyPatrolZone> DesignatedZones = new();

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
        if(DesignatedZones.ContainsKey(enemy))
        {
            DesignatedZones.Remove(enemy);
        }
    }

    /// <summary>
    /// Zwraca punkt patrolowy dla danego wroga.
    /// Jeœli uda siê ustaliæ wyznaczon¹ PatrolZone (na podstawie pozycji oraz osi¹galnoœci),
    /// wróg zawsze bêdzie korzysta³ z punktu z tej strefy.
    /// W przeciwnym razie generowany jest losowy punkt na globalnym NavMesh.
    /// </summary>
    public static Vector3 GetPatrolPoint(EnemyBase enemy)
    {
        SpecifyPatrolZone zone;
        if (!DesignatedZones.TryGetValue(enemy, out zone))
        {
            zone = CalculateDesignatedZoneForEnemy(enemy);
            if(zone != null)
                DesignatedZones[enemy] = zone;
        }

        if (zone != null)
        {
            return zone.GetRandomPatrolPoint(enemy.MinPatrolPointDistance);
        }
        return GetPatrolPoint(enemy.transform.position, enemy.PatrolRange, enemy.MinPatrolPointDistance);
    }

    /// <summary>
    /// Oblicza i zwraca PatrolZone, która jest osi¹galna przez wroga.
    /// Najpierw sprawdzamy, czy wróg jest ju¿ wewn¹trz jakiejœ strefy – wtedy j¹ zwracamy.
    /// Jeœli nie, wybieramy spoœród tych, do których mo¿na obliczyæ kompletn¹ œcie¿kê,
    /// tê, która jest najbli¿sza.
    /// </summary>
    private static SpecifyPatrolZone CalculateDesignatedZoneForEnemy(EnemyBase enemy)
    {
        if (SpecifyPatrolZone.AllZones.Count == 0)
            return null;

        // Jeœli wróg znajduje siê ju¿ wewn¹trz którejœ strefy, natychmiast j¹ zwracamy
        foreach (var zone in SpecifyPatrolZone.AllZones)
        {
            if (zone.ContainsPoint(enemy.transform.position))
                return zone;
        }

        SpecifyPatrolZone bestZone = null;
        float bestDistance = Mathf.Infinity;
        foreach (var zone in SpecifyPatrolZone.AllZones)
        {
            // Próbujemy obliczyæ œcie¿kê do centrum strefy (mo¿na te¿ u¿yæ innego reprezentatywnego punktu)
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(enemy.transform.position, zone.transform.position, NavMesh.AllAreas, path))
            {
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    float distance = Vector3.Distance(enemy.transform.position, zone.transform.position);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestZone = zone;
                    }
                }
            }
        }
        return bestZone;
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
