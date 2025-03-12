using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Enemy.Type;

namespace Enemy.State
{
    public class PatrolState : IEnemyState
    {
        private Vector3 patrolPoint;
        private bool isWaiting;
        private float waitTimer;

        // Machine-specific head rotation
        private Coroutine headRotationCoroutine;
        private int lastHeadPositionIndex;

        public void EnterState(EnemyBase enemy)
        {
            enemy.sound.PlayPatrolSound();
            enemy.navMeshAgent.isStopped = false;
            SetNewPatrolPoint(enemy);
        }

        public void UpdateState(EnemyBase enemy)
        {
            if (isWaiting)
            {
                waitTimer -= Time.deltaTime;
                StartHeadRotation(enemy);
                
                if (IsObjectInFront(enemy))
                {
                    TurnTowardsFreeSpace(enemy);
                }

                if (waitTimer <= 0f)
                {
                    isWaiting = false;
                    ResetHeadRotation(enemy);
                    EnemyPatrolHandler.ReleasePatrolPoint(patrolPoint);
                    SetNewPatrolPoint(enemy);
                }
                return;
            }

            // If the agent has reached the patrol destination
            if (!enemy.navMeshAgent.pathPending &&
                enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)
            {
                // Start waiting and release current patrol point
                isWaiting = true;
                waitTimer = enemy.waitTimeAtPatrolPoint;

                if (patrolPoint != Vector3.zero)
                {
                    EnemyPatrolHandler.ReleasePatrolPoint(patrolPoint);
                    patrolPoint = Vector3.zero;
                }
            }
        }

        public void ExitState(EnemyBase enemy)
        {
            if (!enemy.navMeshAgent.pathPending &&
                enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)
            {
                EnemyPatrolHandler.ReleasePatrolPoint(patrolPoint);
                ResetHeadRotation(enemy);
            }
        }

        private void SetNewPatrolPoint(EnemyBase enemy)
        {
            if (!enemy.navMeshAgent.isOnNavMesh || !enemy.navMeshAgent.enabled)
            {
                Debug.LogError($"[{enemy.name}] Agent is not on NavMesh!");
                return;
            }

            patrolPoint = enemy.RequestPatrolPoint();

            if (NavMesh.SamplePosition(patrolPoint, out NavMeshHit hit, enemy.patrolRange, NavMesh.AllAreas))
            {
                patrolPoint = hit.position;
                Debug.Log($"New patrol point set at: {patrolPoint}");
                if (enemy.canMove)
                {
                    enemy.navMeshAgent.SetDestination(patrolPoint);
                }
            }
        }

        /// <summary>
        /// Casts a ray from the enemy's position (offset by 1.5 units up) in its forward direction.
        /// Returns true if an obstacle is detected within the detection distance.
        /// </summary>
        private bool IsObjectInFront(EnemyBase enemy)
        {
            float mainDetectionDistance = 6f;
            float sideDetectionDistance = 4f;
            float rayOriginHeight = 1.5f;
            float sideRayAngleOffset = 30f;

            // Jeśli enemy jest MachineEnemy, pobieramy ustawienia z jego zmiennych
            if (enemy is MachineEnemy machineEnemy)
            {
                mainDetectionDistance = machineEnemy.mainDetectionDistance;
                sideDetectionDistance = machineEnemy.sideDetectionDistance;
                rayOriginHeight = machineEnemy.rayOriginHeight;
                sideRayAngleOffset = machineEnemy.sideRayAngleOffset;
            }

            Vector3 origin = enemy.transform.position + Vector3.up * rayOriginHeight;
            Vector3 mainDirection = enemy.transform.forward;

            // Główny raycast
            bool mainHit = Physics.Raycast(origin, mainDirection, mainDetectionDistance);
            Debug.DrawRay(origin, mainDirection * mainDetectionDistance, mainHit ? Color.red : Color.green, 0.0f);

            // Raycasty boczne
            Vector3 leftDirection = Quaternion.Euler(0, -sideRayAngleOffset, 0) * enemy.transform.forward;
            Vector3 rightDirection = Quaternion.Euler(0, sideRayAngleOffset, 0) * enemy.transform.forward;
            bool leftHit = Physics.Raycast(origin, leftDirection, sideDetectionDistance);
            bool rightHit = Physics.Raycast(origin, rightDirection, sideDetectionDistance);

            Debug.DrawRay(origin, leftDirection * sideDetectionDistance, leftHit ? Color.red : Color.green, 0.0f);
            Debug.DrawRay(origin, rightDirection * sideDetectionDistance, rightHit ? Color.red : Color.green, 0.0f);

            // Logujemy informację, jeżeli którykolwiek raycast trafił przeszkodę
            if (mainHit || leftHit || rightHit)
            {
                Debug.Log($"Obstacle detected in front or at the sides of {enemy.name}: " +
                          $"Main hit: {mainHit}, Left hit: {leftHit}, Right hit: {rightHit}");
            }

            // Zwracamy true, jeśli choć jeden raycast wykrył przeszkodę
            return mainHit || leftHit || rightHit;
        }


        /// <summary>
        /// Gathers candidate directions from a standard scanning range (±60° with 15° steps)
        /// and, if the enemy is a MachineEnemy, also adds two additional directions defined in MachineEnemy.
        /// Then selects the best candidate:
        /// - Preferably one that is free (no obstacle) and closest to enemy.forward.
        /// - Otherwise, the one with maximum clearance.
        /// Visualizes all extra candidate rays continuously.
        /// </summary>
        private void TurnTowardsFreeSpace(EnemyBase enemy)
        {
            // Use settings from MachineEnemy if available; otherwise, default values.
            float mainDetectionDistance = 6f;
            float sideDetectionDistance = 4f;
            float rayOriginHeight = 1.5f;
            float angleRange = 60f;
            float angleStep = 15f;
            float sideRayAngleOffset = 30f; // default side ray angle

            MachineEnemy machineEnemy = enemy as MachineEnemy;
            if (machineEnemy != null)
            {
                mainDetectionDistance = machineEnemy.mainDetectionDistance;
                sideDetectionDistance = machineEnemy.sideDetectionDistance;
                rayOriginHeight = machineEnemy.rayOriginHeight;
                sideRayAngleOffset = machineEnemy.sideRayAngleOffset;
            }

            Vector3 origin = enemy.transform.position + Vector3.up * rayOriginHeight;
            
            // --- Check side rays continuously ---
            Vector3 leftSideDir = Quaternion.Euler(0, sideRayAngleOffset, 0) * enemy.transform.forward;
            Vector3 rightSideDir = Quaternion.Euler(0, -sideRayAngleOffset, 0) * enemy.transform.forward;
            bool leftSideHit = Physics.Raycast(origin, leftSideDir, sideDetectionDistance);
            bool rightSideHit = Physics.Raycast(origin, rightSideDir, sideDetectionDistance);
            
            Debug.DrawRay(origin, leftSideDir * sideDetectionDistance, leftSideHit ? Color.red : Color.green, 0.0f);
            Debug.DrawRay(origin, rightSideDir * sideDetectionDistance, rightSideHit ? Color.red : Color.green, 0.0f);
            
            // --- Handle ambiguous side situation ---
            if (leftSideHit && !rightSideHit)
            {
                // Left side blocked, turn right.
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(rightSideDir.x, 0, rightSideDir.z));
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, Time.deltaTime * enemy.rotationMultiplier);
                Debug.DrawRay(origin, rightSideDir * mainDetectionDistance, Color.blue, 0.0f);
                Debug.Log($"{enemy.name} turning right due to left side obstacle.");
                return;
            }
            else if (rightSideHit && !leftSideHit)
            {
                // Right side blocked, turn left.
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(leftSideDir.x, 0, leftSideDir.z));
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, Time.deltaTime * enemy.rotationMultiplier);
                Debug.DrawRay(origin, leftSideDir * mainDetectionDistance, Color.blue, 0.0f);
                Debug.Log($"{enemy.name} turning left due to right side obstacle.");
                return;
            }
            else if (leftSideHit && rightSideHit)
            {
                // Both sides are blocked – try turning backward.
                Vector3 backwardDir = -enemy.transform.forward;
                bool backwardHit = Physics.Raycast(origin, backwardDir, mainDetectionDistance);
                Debug.DrawRay(origin, backwardDir * mainDetectionDistance, backwardHit ? Color.red : Color.green, 0.0f);
                if (!backwardHit)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(new Vector3(backwardDir.x, 0, backwardDir.z));
                    enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, Time.deltaTime * enemy.rotationMultiplier);
                    Debug.Log($"{enemy.name} turning backwards due to ambiguous side obstacles.");
                    return;
                }
                else
                {
                    // If backward is also blocked, fallback to turning left.
                    Quaternion targetRotation = Quaternion.LookRotation(new Vector3(leftSideDir.x, 0, leftSideDir.z));
                    enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, Time.deltaTime * enemy.rotationMultiplier);
                    Debug.Log($"{enemy.name} turning left as fallback when backward is blocked.");
                    return;
                }
            }
            
            // --- Standard candidate scanning if sides are clear ---
            List<Vector3> candidateDirections = new List<Vector3>();
            for (float angle = -angleRange; angle <= angleRange; angle += angleStep)
            {
                Quaternion rotation = Quaternion.Euler(0, angle, 0);
                candidateDirections.Add(rotation * enemy.transform.forward);
            }
            
            // For MachineEnemy add additional candidate directions.
            if (machineEnemy != null)
            {
                Vector3 extraRightDir = Quaternion.Euler(0, machineEnemy.additionalRaycastAngleOffset, 0) * enemy.transform.forward;
                Vector3 extraLeftDir = Quaternion.Euler(0, -machineEnemy.additionalRaycastAngleOffset, 0) * enemy.transform.forward;
                candidateDirections.Add(extraRightDir);
                candidateDirections.Add(extraLeftDir);
                Debug.DrawRay(origin, extraRightDir * mainDetectionDistance, Color.cyan, 0.0f);
                Debug.DrawRay(origin, extraLeftDir * mainDetectionDistance, Color.cyan, 0.0f);
            }
            
            // Choose free candidates.
            List<Vector3> freeCandidates = new List<Vector3>();
            foreach (Vector3 candidate in candidateDirections)
            {
                if (!Physics.Raycast(origin, candidate, mainDetectionDistance))
                {
                    freeCandidates.Add(candidate);
                }
            }
            
            Vector3 bestDirection;
            if (freeCandidates.Count > 0)
            {
                // Choose the free candidate closest to enemy.forward.
                bestDirection = freeCandidates[0];
                float minAngle = Vector3.Angle(enemy.transform.forward, bestDirection);
                foreach (Vector3 candidate in freeCandidates)
                {
                    float angleDiff = Vector3.Angle(enemy.transform.forward, candidate);
                    if (angleDiff < minAngle)
                    {
                        minAngle = angleDiff;
                        bestDirection = candidate;
                    }
                }
            }
            else
            {
                // If no candidate is free, choose the one with maximum clearance.
                bestDirection = candidateDirections[0];
                float maxDistance = 0f;
                foreach (Vector3 candidate in candidateDirections)
                {
                    if (Physics.Raycast(origin, candidate, out RaycastHit hit, mainDetectionDistance))
                    {
                        if (hit.distance > maxDistance)
                        {
                            maxDistance = hit.distance;
                            bestDirection = candidate;
                        }
                    }
                }
            }
            
            Quaternion finalRotation = Quaternion.LookRotation(new Vector3(bestDirection.x, 0, bestDirection.z));
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, finalRotation, Time.deltaTime * enemy.rotationMultiplier);
            Debug.DrawRay(origin, bestDirection * mainDetectionDistance, Color.blue, 0.0f);
            Debug.Log($"{enemy.name} rotated towards free space with direction: {bestDirection}");
        }


        // Head rotation methods for MachineEnemy types
        private void StartHeadRotation(EnemyBase enemy)
        {
            if (enemy is not MachineEnemy machineEnemy) return;
            if (headRotationCoroutine == null)
            {
                headRotationCoroutine = enemy.StartCoroutine(HeadRotationRoutine(machineEnemy));
            }
        }

        private IEnumerator HeadRotationRoutine(MachineEnemy machineEnemy)
        {
            float maxDistance = machineEnemy.maxOffsetDistance;
            int stopPoints = machineEnemy.stopPoints;
            float stopDuration = machineEnemy.stopDuration;
            float headRotationSpeed = machineEnemy.headRotationSpeed;
            float headDetectionDistance = machineEnemy.headDetectionDistance;

            List<Vector3> movementOffsets = new List<Vector3>();
            for (int i = 0; i < stopPoints; i++)
            {
                float offset = Mathf.Lerp(-maxDistance, maxDistance, i / (float)(stopPoints - 1));
                movementOffsets.Add(new Vector3(offset, 0f, 0f));
            }
            for (int i = stopPoints - 2; i >= 0; i--)
            {
                movementOffsets.Add(movementOffsets[i]);
            }

            int direction = 1;
            yield return new WaitForSeconds(1f);

            while (isWaiting)
            {
                if (movementOffsets.Count == 0)
                {
                    Debug.LogWarning("No available head rotation offsets! Exiting head rotation loop.");
                    yield break;
                }

                Vector3 targetLocalPosition = machineEnemy.OriginalHeadPos + movementOffsets[lastHeadPositionIndex];
                float elapsedTime = 0f;
                Vector3 startLocalPosition = machineEnemy.sightTarget.transform.localPosition;
                float transitionDuration = headRotationSpeed;

                while (elapsedTime < transitionDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsedTime / transitionDuration);
                    machineEnemy.sightTarget.transform.localPosition = Vector3.Lerp(startLocalPosition, targetLocalPosition, t);
                    yield return null;
                }

                machineEnemy.sightTarget.transform.localPosition = targetLocalPosition;

                // Check if head is facing an object within the specified detection distance
                Vector3 headWorldPos = machineEnemy.sightTarget.transform.position;
                Vector3 headForward = machineEnemy.sightTarget.transform.forward;
                bool headHit = Physics.Raycast(headWorldPos, headForward, headDetectionDistance);
                Debug.DrawRay(headWorldPos, headForward * headDetectionDistance, headHit ? Color.magenta : Color.cyan, 0.0f);
                
                yield return new WaitForSeconds(stopDuration);
                
                if (lastHeadPositionIndex == movementOffsets.Count - 1 && direction == 1)
                {
                    direction = -1;
                }
                else if (lastHeadPositionIndex == 0 && direction == -1)
                {
                    direction = 1;
                }
                lastHeadPositionIndex += direction;
            }
        }

        private void ResetHeadRotation(EnemyBase enemy)
        {
            if (enemy is MachineEnemy machineEnemy)
            {
                if (headRotationCoroutine != null)
                {
                    enemy.StopCoroutine(headRotationCoroutine);
                    headRotationCoroutine = null;
                }
                enemy.StartCoroutine(SmoothResetPosition(machineEnemy));
            }
        }

        private IEnumerator SmoothResetPosition(MachineEnemy machineEnemy)
        {
            float duration = machineEnemy.headRotationSpeed;
            Vector3 startPosition = machineEnemy.sightTarget.transform.localPosition;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
                machineEnemy.sightTarget.transform.localPosition = Vector3.Lerp(startPosition, machineEnemy.OriginalHeadPos, t);
                yield return null;
            }
            machineEnemy.sightTarget.transform.localPosition = machineEnemy.OriginalHeadPos;
        }
    }
}
