using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy {
    [RequireComponent(typeof(NavMeshAgent))]
    public class MovementController : MonoBehaviour 
    {
        [Header("Reference")] 
        public NavMeshAgent agent;
        [Header("Settings")] 
        public float rotationSpeed = 5f;
        
        [Header("Wall Avoiding")] 
        public float mainDetectionDistance = 6f;
        public float sideDetectionDistance = 4f;
        public float rayOriginHeight = 1.5f;
        public float sideRayAngleOffset = 30f;
        public float additionalRayAngleOffset = 20f;
        
        [Header("Debug")] 
        public bool debugWallRays = true;


        void Awake() => agent = GetComponent<NavMeshAgent>();

        public void Stop() => agent.isStopped = true;
        public void Resume() => agent.isStopped = false;
        public void GoTo(Vector3 pos) { agent.SetDestination(pos); Resume(); }
        
        // -- Rotation methods --

        /// <summary>
        /// Returns true if an obstacle is directly in front or slightly to the sides.
        /// </summary>
        public bool IsObjectInFront()
        {
            Vector3 origin = transform.position + Vector3.up * rayOriginHeight;
            // Main ray
            bool mainHit = Physics.Raycast(origin, transform.forward, mainDetectionDistance);
            Debug.DrawRay(origin, transform.forward * mainDetectionDistance, mainHit ? Color.red : Color.green);

            // Side rays
            Vector3 leftDir = Quaternion.Euler(0, -sideRayAngleOffset, 0) * transform.forward;
            Vector3 rightDir = Quaternion.Euler(0, sideRayAngleOffset, 0) * transform.forward;
            bool leftHit = Physics.Raycast(origin, leftDir, sideDetectionDistance);
            bool rightHit = Physics.Raycast(origin, rightDir, sideDetectionDistance);
            Debug.DrawRay(origin, leftDir * sideDetectionDistance, leftHit ? Color.red : Color.green);
            Debug.DrawRay(origin, rightDir * sideDetectionDistance, rightHit ? Color.red : Color.green);

            return mainHit || leftHit || rightHit;
        }

        /// <summary>
        /// Rotates the enemy towards the clearest direction when blocked.
        /// </summary>
        public void TurnTowardsFreeSpace()
        {
            const float angleRange = 60f;
            const float angleStep = 15f;
            Vector3 origin = transform.position + Vector3.up * rayOriginHeight;

            // Check side blockage
            Vector3 leftSideDir = Quaternion.Euler(0, sideRayAngleOffset, 0) * transform.forward;
            Vector3 rightSideDir = Quaternion.Euler(0, -sideRayAngleOffset, 0) * transform.forward;
            bool leftBlocked = Physics.Raycast(origin, leftSideDir, sideDetectionDistance);
            bool rightBlocked = Physics.Raycast(origin, rightSideDir, sideDetectionDistance);
            Debug.DrawRay(origin, leftSideDir * sideDetectionDistance, leftBlocked ? Color.red : Color.green);
            Debug.DrawRay(origin, rightSideDir * sideDetectionDistance, rightBlocked ? Color.red : Color.green);

            // Simple turn when one side is blocked
            if (leftBlocked && !rightBlocked)
            {
                Quaternion targetRot = Quaternion.LookRotation(new Vector3(rightSideDir.x, 0, rightSideDir.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
                return;
            }
            if (rightBlocked && !leftBlocked)
            {
                Quaternion targetRot = Quaternion.LookRotation(new Vector3(leftSideDir.x, 0, leftSideDir.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
                return;
            }
            if (leftBlocked)
            {
                // Both sides blocked: try backward
                Vector3 backDir = -transform.forward;
                bool backBlocked = Physics.Raycast(origin, backDir, mainDetectionDistance);
                Debug.DrawRay(origin, backDir * mainDetectionDistance, backBlocked ? Color.red : Color.green);
                if (!backBlocked)
                {
                    Quaternion targetRot = Quaternion.LookRotation(new Vector3(backDir.x, 0, backDir.z));
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
                    return;
                }
                // Fallback to left
                Quaternion fallbackRot = Quaternion.LookRotation(new Vector3(leftSideDir.x, 0, leftSideDir.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, fallbackRot, Time.deltaTime * rotationSpeed);
                return;
            }

            // Scan candidate directions
            List<Vector3> candidates = new List<Vector3>();
            for (float a = -angleRange; a <= angleRange; a += angleStep)
            {
                candidates.Add(Quaternion.Euler(0, a, 0) * transform.forward);
            }
            // Add extra offsets
            candidates.Add(Quaternion.Euler(0, additionalRayAngleOffset, 0) * transform.forward);
            candidates.Add(Quaternion.Euler(0, -additionalRayAngleOffset, 0) * transform.forward);

            // Pick free paths
            List<Vector3> free = new List<Vector3>();
            foreach (var dir in candidates)
            {
                if (!Physics.Raycast(origin, dir, mainDetectionDistance)) free.Add(dir);
            }

            Vector3 best;
            if (free.Count > 0)
            {
                best = free[0]; float minAng = Vector3.Angle(transform.forward, best);
                foreach (var dir in free)
                {
                    float ang = Vector3.Angle(transform.forward, dir);
                    if (ang < minAng) { minAng = ang; best = dir; }
                }
            }
            else
            {
                best = candidates[0]; float maxDist = 0f;
                foreach (var dir in candidates)
                {
                    if (Physics.Raycast(origin, dir, out RaycastHit hit, mainDetectionDistance) && hit.distance > maxDist)
                    {
                        maxDist = hit.distance; best = dir;
                    }
                }
            }

            Quaternion finalRot = Quaternion.LookRotation(new Vector3(best.x, 0, best.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, finalRot, Time.deltaTime * rotationSpeed);
            Debug.DrawRay(origin, best * mainDetectionDistance, Color.blue);
        }
        
        public void Face(Vector3 point)
        {
            Vector3 dir = (point - transform.position).normalized;
            var targetRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!debugWallRays) return;
            Vector3 origin = transform.position + Vector3.up * rayOriginHeight;
            var forward = transform.forward;
            // Main forward ray
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(origin, forward * mainDetectionDistance);

            // Side rays
            Gizmos.color = Color.cyan;
            Vector3 leftDir  = Quaternion.Euler(0, -sideRayAngleOffset, 0) * forward;
            Vector3 rightDir = Quaternion.Euler(0,  sideRayAngleOffset, 0) * forward;
            Gizmos.DrawRay(origin, leftDir * sideDetectionDistance);
            Gizmos.DrawRay(origin, rightDir * sideDetectionDistance);

            // Additional machine-specific rays
            Gizmos.color = Color.magenta;
            Vector3 extraL = Quaternion.Euler(0, -additionalRayAngleOffset, 0) * forward;
            Vector3 extraR = Quaternion.Euler(0,  additionalRayAngleOffset, 0) * forward;
            Gizmos.DrawRay(origin, extraL * mainDetectionDistance);
            Gizmos.DrawRay(origin, extraR * mainDetectionDistance);
        }
    }
}