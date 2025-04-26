using System.Collections;
using Anxiety;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.State
{
    public class InvestigateState : IEnemyState
    {
        private Vector3 _lastKnownPosition;
        private bool _fearIncreased;

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
        }

        public void UpdateState(EnemyBrain enemyBrain)
        {
            if (enemyBrain.detection.IsPlayerVisible && enemyBrain.detection.AwarenessLevel < enemyBrain.detectionValueToChase)
            {
                enemyBrain.movement.Stop();
                enemyBrain.movement.Face(_lastKnownPosition);
                if (_fearIncreased) return;
                AnxietyManager.Instance.IncreaseFear(5f);
                _fearIncreased = true;
                return; 
            }
            
            if (enemyBrain.Target != null && enemyBrain.detection.IsPlayerVisible)
            {
                _lastKnownPosition = enemyBrain.Target.position;
                
                enemyBrain.movement.Face(_lastKnownPosition);

                if (enemyBrain.canMove && 
                    enemyBrain.IsTargetInNavMesh(out NavMeshHit hit) &&
                    enemyBrain.detection.AwarenessLevel > enemyBrain.detectionValueToChase)
                {
                    enemyBrain.movement.GoTo(hit.position);
                }
                return;
            }
        }

        public void ExitState(EnemyBrain enemyBrain)
        {
            _fearIncreased = false;
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