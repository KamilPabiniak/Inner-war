using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Define one or more 3D volumes (BoxColliders) as patrol zones.
/// Attach this to a GameObject, assign its BoxCollider children or references,
/// and optionally reference it from EnemyBrain to constrain patrol points.
/// </summary>
[DisallowMultipleComponent]
public class PatrolArea : MonoBehaviour
{
    [Tooltip("List of BoxCollider volumes that define patrol zones. These should be set as Triggers.")]
    public List<BoxCollider> zones = new List<BoxCollider>();

    // Track occupied points within these zones to avoid overlap
    private readonly List<Vector3> _occupied = new List<Vector3>();

    /// <summary>
    /// Samples a random point inside one of the defined zones, avoiding too-close points.
    /// </summary>
    /// <param name="minDist">Minimum distance from any other occupied point.</param>
    /// <param name="attempts">Number of sampling attempts before giving up.</param>
    /// <returns>A valid patrol point, or the GameObject position if none found.</returns>
    public Vector3 GetRandomPatrolPoint(float minDist, int attempts = 5)
    {
        if (zones == null || zones.Count == 0)
            return transform.position;

        for (int i = 0; i < attempts; i++)
        {
            // Pick a random zone
            var zone = zones[Random.Range(0, zones.Count)];
            if (zone == null) continue;

            // Calculate a random point within the BoxCollider in world space
            Vector3 localCenter = zone.center;
            Vector3 extents = zone.size * 0.5f;
            float x = Random.Range(-extents.x, extents.x);
            float y = Random.Range(-extents.y, extents.y);
            float z = Random.Range(-extents.z, extents.z);
            Vector3 localPoint = localCenter + new Vector3(x, y, z);
            Vector3 worldPoint = zone.transform.TransformPoint(localPoint);

            // Sample NavMesh to get a valid position
            if (NavMesh.SamplePosition(worldPoint, out var hit, Mathf.Max(extents.magnitude, 1f), NavMesh.AllAreas))
            {
                Vector3 candidate = hit.position;
                if (IsValid(candidate, minDist))
                {
                    _occupied.Add(candidate);
                    return candidate;
                }
            }
        }

        // Fallback
        return transform.position;
    }

    /// <summary>
    /// Releases an occupied patrol point so it can be reused.
    /// </summary>
    public void ReleasePoint(Vector3 point)
    {
        _occupied.RemoveAll(p => Vector3.Distance(p, point) < 0.01f);
    }

    private bool IsValid(Vector3 point, float minDist)
    {
        foreach (var occ in _occupied)
            if (Vector3.Distance(occ, point) < minDist)
                return false;
        return true;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (zones == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        foreach (var zone in zones)
        {
            if (zone == null) continue;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = zone.transform.localToWorldMatrix;
            Gizmos.DrawCube(zone.center, zone.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(zone.center, zone.size);
            Gizmos.matrix = oldMatrix;
        }
    }
#endif

}
