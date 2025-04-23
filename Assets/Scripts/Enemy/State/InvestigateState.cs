using System.Collections;
using Anxiety;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.State
{
    public class InvestigateState : IEnemyState
    {
        private Vector3 _lastKnownPosition;
        private Coroutine _headRotationCoroutine;
        private float _lostSightTimer;

        private readonly float _initialRotationTime = 1.5f; 
        public InvestigateState(Vector3 position)
        {
            _lastKnownPosition  = position;
        }
        public void UpdatePosition(Vector3 newPosition)
        {
            _lastKnownPosition = newPosition;
        }

        public void EnterState(EnemyBrain enemyBrain)
        {
            enemyBrain.StartCoroutine(LookAtAlert(enemyBrain));
            enemyBrain.audio.PlayInvestigateSound();
            AnxietyManager.Instance.IncreaseFear(5f);
        }

        public void UpdateState(EnemyBrain enemyBrain)
        {
            if (enemyBrain.detection.IsPlayerVisible && enemyBrain.detection.AwarenessLevel < enemyBrain.detectionValueToChase)
            {
                enemyBrain.movement.Stop();
                enemyBrain.movement.Face(enemyBrain.Target.position);
                return; 
            }
            
            if (enemyBrain.Target != null && enemyBrain.detection.IsPlayerVisible)
            {
                _lostSightTimer = 0f;
                _lastKnownPosition = enemyBrain.Target.position;

                if (enemyBrain.IsTargetInNavMesh(out NavMeshHit hit))
                {
                    if (!enemyBrain.canMove || !(enemyBrain.detection.AwarenessLevel > enemyBrain.detectionValueToChase)) return;
                    enemyBrain.movement.Face(enemyBrain.Target.position);
                    enemyBrain.movement.GoTo(hit.position);
                }
                else
                {
                    enemyBrain.movement.Face(enemyBrain.Target.position);
                }
            }
            else
            {
                _lostSightTimer += Time.deltaTime;
            
                if (_lostSightTimer < enemyBrain.maxInvestigationTime)
                {
                    if (enemyBrain.canMove && enemyBrain.IsTargetInNavMesh(out _) && enemyBrain.detection.AwarenessLevel > enemyBrain.detectionValueToChase)
                    {
                        enemyBrain.movement.agent.SetDestination(_lastKnownPosition);
                    }
                }
            }
        }

        public void ExitState(EnemyBrain enemyBrain)
        {
        }
    
        private IEnumerator LookAtAlert(EnemyBrain enemyBrain)
        {
            float timer = 0f;
            while (timer < _initialRotationTime)
            {
                RotateToAlert(enemyBrain);
                timer += Time.deltaTime;
                yield return null;
            }
        }
    
        private void RotateToAlert(EnemyBrain enemyBrain)
        {
            Vector3 direction = (_lastKnownPosition - enemyBrain.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            enemyBrain.transform.rotation = Quaternion.Slerp(enemyBrain.transform.rotation, lookRotation, Time.deltaTime * enemyBrain.movement.rotationSpeed);
        }
    }
}