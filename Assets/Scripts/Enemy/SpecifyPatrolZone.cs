using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpecifyPatrolZone : MonoBehaviour
{
    [Tooltip("List of colliders defining the patrol area. You can add BoxCollider, SphereCollider, etc.")]
    public List<Collider> patrolAreas = new();

    // Static list of all patrol zones in the scene
    public static readonly List<SpecifyPatrolZone> AllZones = new();

    private void Awake()
    {
        if (!AllZones.Contains(this))
            AllZones.Add(this);
    }

    private void OnDestroy()
    {
        if (AllZones.Contains(this))
            AllZones.Remove(this);
    }

    /// <summary>
    /// Checks if the given point is inside any of the colliders of this zone.
    /// </summary>
    public bool ContainsPoint(Vector3 point)
    {
        if (patrolAreas == null || patrolAreas.Count == 0)
            return false;

        foreach (var col in patrolAreas)
        {
            // If the closest point on the collider equals the given point, it's inside.
            if (col.ClosestPoint(point) == point)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns a random patrol point within one of the defined colliders,
    /// ensuring the point is on the NavMesh and not too close to already occupied points.
    /// Instead of calling OccupyPoint, we simply use GetPatrolPoint logic.
    /// </summary>
    public Vector3 GetRandomPatrolPoint(float minDistance)
    {
        if (patrolAreas == null || patrolAreas.Count == 0)
        {
            Debug.LogWarning("No patrol areas assigned in PatrolZone: " + gameObject.name);
            return transform.position;
        }

        int maxAttempts = 10;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Select a random collider from the list
            Collider col = patrolAreas[Random.Range(0, patrolAreas.Count)];
            Vector3 randomPoint = Vector3.zero;

            if (col is BoxCollider box)
            {
                var bounds = box.bounds;
                Vector3 center = bounds.center;
                Vector3 extents = bounds.extents;
                randomPoint = new Vector3(
                    Random.Range(center.x - extents.x, center.x + extents.x),
                    center.y,
                    Random.Range(center.z - extents.z, center.z + extents.z)
                );
            }
            else if (col is SphereCollider sphere)
            {
                Vector3 center = sphere.bounds.center;
                float radius = sphere.radius;
                Vector2 randomCircle = Random.insideUnitCircle * radius;
                randomPoint = new Vector3(center.x + randomCircle.x, center.y, center.z + randomCircle.y);
            }
            else
            {
                // Default: choose a point within the collider's bounds
                Vector3 center = col.bounds.center;
                Vector3 extents = col.bounds.extents;
                randomPoint = new Vector3(
                    Random.Range(center.x - extents.x, center.x + extents.x),
                    center.y,
                    Random.Range(center.z - extents.z, center.z + extents.z)
                );
            }

            // Ensure the point is on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
            {
                Vector3 navPoint = hit.position;
                // Check if this point is valid (not too close to other patrol points)
                if (EnemyPatrolHandler.IsPointValid(navPoint, minDistance))
                {
                    // Instead of OccupyPoint, we simply return the point.
                    return navPoint;
                }
            }
        }
        // Fallback: return current position if no valid point found
        return transform.position;
    }

    private void OnDrawGizmos()
    {
        if (patrolAreas == null || patrolAreas.Count == 0)
            return;

        Gizmos.color = Color.yellow;
        foreach (var col in patrolAreas)
        {
            if (!col) return;
            if (col is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
            }
        }
    }
}
