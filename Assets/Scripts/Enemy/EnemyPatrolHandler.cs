using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public static class EnemyPatrolHandler
    {
        private static readonly List<EnemyBrain> Registered = new();
        private static readonly List<Vector3> Occupied = new();

        public static void RegisterEnemy(EnemyBrain e)
        {
            if (!Registered.Contains(e))
                Registered.Add(e);
        }

        public static void UnregisterEnemy(EnemyBrain e)
        {
            Registered.Remove(e);
        }

        public static Vector3 GetPatrolPoint(Vector3 origin, float range, float minDist)
        {
            // Try random directions within the patrol radius
            for (int i = 0; i < 10; i++)
            {
                Vector3 randDir = Random.insideUnitSphere * range;
                randDir.y = 0f;
                Vector3 candidate = origin + randDir;

                // Ensure point lies on NavMesh
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1f, NavMesh.AllAreas))
                {
                    Vector3 p = hit.position;
                    // Check actual distance and occupancy
                    if (Vector3.Distance(origin, p) <= range && IsValid(p, minDist))
                    {
                        Occupied.Add(p);
                        return p;
                    }
                }
            }

            // No valid patrol point found: stay in place
            return origin;
        }

        private static bool IsValid(Vector3 p, float minDist)
        {
            float minDistSqr = minDist * minDist;
            foreach (var o in Occupied)
            {
                if ((o - p).sqrMagnitude < minDistSqr)
                    return false;
            }
            return true;
        }

        public static void ReleasePatrolPoint(Vector3 p)
        {
            if (Occupied.Contains(p))
                Occupied.Remove(p);
        }
    }
}