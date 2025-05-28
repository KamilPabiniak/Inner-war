using Anxiety;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.State
{
    public class InvestigateState : IEnemyState
    {
        private Vector3 _lastKnownPosition;
        private bool _fearIncreased;
        
        private static readonly int Speed = Animator.StringToHash("Speed");
        
        public void UpdatePosition(Vector3 newPosition)
        {
            _lastKnownPosition = newPosition;
        }

        public void EnterState(EnemyBrain enemyBrain)
        {
            enemyBrain.audio.PlayInvestigateSound();
        }

        public void UpdateState(EnemyBrain enemyBrain)
        {
            if (enemyBrain.detection.IsPlayerVisible )
            {
                AnxietyManager.Instance.TriggerActiveContinuous(3);   
            }
            
            float vel = enemyBrain.movement.agent.velocity.magnitude;
            enemyBrain.animator.SetFloat(Speed, vel * enemyBrain.animationWalkSpeedInvestigate); 
            
            if (enemyBrain.detection.IsPlayerVisible 
                && enemyBrain.detection.AwarenessLevel < enemyBrain.detectionValueToChase)
            {
                enemyBrain.movement.Stop();
                enemyBrain.movement.Face(_lastKnownPosition);
                if (!_fearIncreased)
                {
                    AnxietyManager.Instance.ChangeFear(AnxietyManager.Instance.increaseFearValueOnSpotted);
                    _fearIncreased = true;
                }
                return;
            }
            
            if (!enemyBrain.detection.IsPlayerVisible)
            {
                if (enemyBrain.canMove)
                {
                    enemyBrain.movement.GoTo(_lastKnownPosition);
                    enemyBrain.movement.Face(_lastKnownPosition);
                }
                return;
            }
            
            if (enemyBrain.Target != null && enemyBrain.detection.IsPlayerVisible)
            {
                _lastKnownPosition = enemyBrain.Target.position;
                enemyBrain.movement.Face(_lastKnownPosition);

                if (enemyBrain.canMove 
                    && enemyBrain.IsTargetInNavMesh(out NavMeshHit hit) 
                    && enemyBrain.detection.AwarenessLevel > enemyBrain.detectionValueToChase)
                {
                    enemyBrain.movement.GoTo(hit.position);
                }
            }
        }

        public void ExitState(EnemyBrain enemyBrain)
        {
            _fearIncreased = false;
        }
    }
}