using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy 
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovement : MonoBehaviour 
    {
        [Header("Reference")] 
        public NavMeshAgent agent;
        [Header("Settings")] 
        public float rotationSpeed = 5f;
        
        [Header("Wall Avoiding")] 
        public float detectionDistance = 6f;
        public float rayOriginHeight = 1.5f;
        
        [Header("Throttling")]
        [Tooltip("Minimum distance change to issue a new SetDestination")]
        [SerializeField] private float destThreshold = 0.5f;
        [Tooltip("Minimum time interval between SetDestination calls")]
        [SerializeField] private float setInterval = 0.2f;
        private Vector3 _lastDestination;
        private float _lastSetTime;
        
        [Header("Debug")] 
        public bool debugRays = true;

        void Awake() => agent = GetComponent<NavMeshAgent>();

        public void Stop() => agent.isStopped = true;
        public void Resume() => agent.isStopped = false;

        /// <summary>
        /// Moves the agent to the given position, but throttles calls to SetDestination
        /// to avoid performance hitches.
        /// </summary>
        public void GoTo(Vector3 pos)
        {
            if (!agent.isOnNavMesh || !agent.enabled) 
                return;

            float now = Time.time;
            float dist = Vector3.Distance(pos, _lastDestination);

            // Only issue a new SetDestination if the target moved enough or enough time passed
            if (dist > destThreshold || now - _lastSetTime > setInterval)
            {
                agent.SetDestination(pos);
                _lastDestination = pos;
                _lastSetTime = now;
            }

            Resume();
        }
        
        // -- Rotation methods --

        public bool IsObstacleInFront(out RaycastHit hitInfo)
        {
            Vector3 origin = transform.position + Vector3.up * rayOriginHeight;
            Vector3 dir = transform.forward;
            float radius = agent.radius;

            bool hit = Physics.SphereCast(origin, radius, dir, out hitInfo, detectionDistance);
            if (debugRays)
                Debug.DrawRay(origin, dir * detectionDistance, hit ? Color.red : Color.green);

            return hit;
        }

        /// <summary>
        /// Rotates smoothly using steering: reflects forward vector on hit normal.
        /// </summary>
        public void TurnTowardsFreeSpace()
        {
            if (IsObstacleInFront(out RaycastHit hit))
            {
                // Calculate steering vector: reflect forward off obstacle normal
                Vector3 reflectDir = Vector3.Reflect(transform.forward, hit.normal).normalized;
                Quaternion targetRot = Quaternion.LookRotation(reflectDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);

                if (debugRays)
                    Debug.DrawRay(hit.point, hit.normal, Color.yellow);
            }
            else
            {
                // No obstacle: align with desired velocity (NavMesh)
                Vector3 desired = agent.velocity;
                if (desired.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(desired.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
                }
            }
        }
        
        public void Face(Vector3 point)
        {
            point.y = transform.position.y;
        
            Vector3 dir = (point - transform.position).normalized;
            if (dir == Vector3.zero) return; 
        
            var targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRot, 
                Time.deltaTime * rotationSpeed * 2f 
            );
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!debugRays || agent == null) return;
            Vector3 origin = transform.position + Vector3.up * rayOriginHeight;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(origin + transform.forward * detectionDistance, agent.radius);
        }
    }
}
