using Anxiety;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.State
{
    public class InvestigateState : IEnemyState
    {
        private Vector3 _lastKnownPosition;
        private bool _fearIncreased;
        
        public void UpdatePosition(Vector3 newPosition)
        {
            _lastKnownPosition = newPosition;
        }

        public void EnterState(EnemyBrain enemyBrain)
        {
            enemyBrain.audio.PlayInvestigateSound();
            if (enemyBrain.detection.IsPlayerVisible)
            {
                AnxietyManager.Instance.TriggerActiveContinuous(3);   
            }
        }

        public void UpdateState(EnemyBrain enemyBrain)
        {
            if (enemyBrain.detection.IsPlayerVisible 
                && enemyBrain.detection.AwarenessLevel < enemyBrain.detectionValueToChase)
            {
                enemyBrain.enemyMovement.Stop();
                enemyBrain.enemyMovement.Face(_lastKnownPosition);
                if (!_fearIncreased)
                {
                    AnxietyManager.Instance.AddFear(AnxietyManager.Instance.increaseFearValueOnSpotted);
                    _fearIncreased = true;
                }
                return;
            }
            
            if (!enemyBrain.detection.IsPlayerVisible)
            {
                if (enemyBrain.canMove)
                {
                    enemyBrain.enemyMovement.GoTo(_lastKnownPosition);
                    enemyBrain.enemyMovement.Face(_lastKnownPosition);
                }
                return;
            }
            
            if (enemyBrain.Target != null && enemyBrain.detection.IsPlayerVisible)
            {
                _lastKnownPosition = enemyBrain.Target.position;
                enemyBrain.enemyMovement.Face(_lastKnownPosition);

                if (enemyBrain.canMove 
                    && enemyBrain.IsTargetInNavMesh(out NavMeshHit hit) 
                    && enemyBrain.detection.AwarenessLevel > enemyBrain.detectionValueToChase)
                {
                    enemyBrain.enemyMovement.GoTo(hit.position);
                }
            }
        }

        public void ExitState(EnemyBrain enemyBrain)
        {
            _fearIncreased = false;
        }
    }
}