using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public static class EnemyPatrolHandler
    {
        private static readonly List<EnemyBase> Registered = new();
        private static readonly List<Vector3> Occupied = new();

        public static void RegisterEnemy(EnemyBase e) { if (!Registered.Contains(e)) Registered.Add(e); }
        public static void UnregisterEnemy(EnemyBase e) { Registered.Remove(e); }

        public static Vector3 GetPatrolPoint(EnemyBase e) =>
            GetPatrolPoint(e.transform.position, EnemyBase.PatrolRange, EnemyBase.MinPatrolPointDistance);

        private static Vector3 GetPatrolPoint(Vector3 origin, float range, float minDist)
        {
            var nav = NavMesh.CalculateTriangulation();
            for (int i = 0; i < 5; i++)
            {
                int t = Random.Range(0, nav.indices.Length / 3) * 3;
                var v1 = nav.vertices[nav.indices[t]];
                var v2 = nav.vertices[nav.indices[t + 1]];
                var v3 = nav.vertices[nav.indices[t + 2]];
                Vector3 p = RandomPointInTriangle(v1, v2, v3);
                if (Vector3.Distance(origin, p) <= range && IsValid(p, minDist))
                {
                    Occupied.Add(p);
                    return p;
                }
            }
            return origin;
        }

        private static Vector3 RandomPointInTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            float u = Random.value, v = Random.value;
            if (u + v > 1f) { u = 1 - u; v = 1 - v; }
            return u * a + v * b + (1 - u - v) * c;
        }

        private static bool IsValid(Vector3 p, float minDist)
        {
            foreach (var o in Occupied)
                if (Vector3.Distance(o, p) < minDist) return false;
            return true;
        }

        public static void ReleasePatrolPoint(Vector3 p) => Occupied.Remove(p);
    }
}