using UnityEngine;
using UnityEngine.AI;

namespace Enemy.State
{
    public class PatrolState : IEnemyState
    {
        private Vector3 _patrolPoint;
        private bool _isWaiting;                
        private bool _waitingToMove;              
        private float _waitTimer;

        private static readonly int CheckArea = Animator.StringToHash("Search");
        private static readonly int Speed     = Animator.StringToHash("Speed");

        public void EnterState(EnemyBrain enemyBrain)
        {
            enemyBrain.audio.PlayPatrolSound();
            enemyBrain.movement.Resume();
            _isWaiting = false;
            _waitingToMove = false;
            SetNewPatrolPoint(enemyBrain);
        }

        public void UpdateState(EnemyBrain enemyBrain)
        {
            float vel = enemyBrain.movement.agent.velocity.magnitude;
            enemyBrain.animator.SetFloat(Speed, vel * enemyBrain.animationWalkSpeedPatrol);
            
            if (_waitingToMove)
            {
                _waitTimer -= Time.deltaTime;
                enemyBrain.animator.SetBool(CheckArea, true);
                
                // Vector3 dir = (_patrolPoint - enemyBrain.transform.position).normalized;
                // enemyBrain.transform.forward = Vector3.Lerp(enemyBrain.transform.forward, dir, Time.deltaTime * enemyBrain.rotationSpeed);

                if (_waitTimer <= 0f)
                {
                    enemyBrain.animator.SetBool(CheckArea, false);
                    _waitingToMove = false;
                    if (enemyBrain.canMove)
                    {
                        enemyBrain.movement.GoTo(_patrolPoint);
                    }
                }
                return;
            }
            
            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                enemyBrain.animator.SetBool(CheckArea, true);

                if (enemyBrain.movement.IsObjectInFront())
                    enemyBrain.movement.TurnTowardsFreeSpace();

                if (_waitTimer <= 0f)
                {
                    enemyBrain.animator.SetBool(CheckArea, false);
                    _isWaiting = false;
                    
                    if (_patrolPoint != enemyBrain.transform.position)
                    {
                        if (enemyBrain.patrolAreaOverride != null)
                            enemyBrain.patrolAreaOverride.ReleasePoint(_patrolPoint);
                        else
                            EnemyPatrolHandler.ReleasePatrolPoint(_patrolPoint);
                    }

                    SetNewPatrolPoint(enemyBrain);
                }
                return;
            }
            
            if (!enemyBrain.movement.agent.pathPending &&
                enemyBrain.movement.agent.remainingDistance <= enemyBrain.movement.agent.stoppingDistance)
            {
                _isWaiting = true;
                _waitTimer = enemyBrain.waitTimeAtPatrolPoint;
            }
        }

        public void ExitState(EnemyBrain enemyBrain)
        {
            enemyBrain.animator.SetBool(CheckArea, false);

            // Clean up current point if standing on it
            if (!enemyBrain.movement.agent.pathPending &&
                enemyBrain.movement.agent.remainingDistance <= enemyBrain.movement.agent.stoppingDistance &&
                _patrolPoint != enemyBrain.transform.position)
            {
                if (enemyBrain.patrolAreaOverride != null)
                    enemyBrain.patrolAreaOverride.ReleasePoint(_patrolPoint);
                else
                    EnemyPatrolHandler.ReleasePatrolPoint(_patrolPoint);
            }
        }

        private void SetNewPatrolPoint(EnemyBrain enemyBrain)
        {
            if (!enemyBrain.movement.agent.isOnNavMesh || !enemyBrain.movement.agent.enabled)
                return;

            _patrolPoint = enemyBrain.patrolAreaOverride != null
                ? enemyBrain.patrolAreaOverride.GetRandomPatrolPoint(enemyBrain.minPatrolPointDistance)
                : enemyBrain.RequestPatrolPoint();
            
            if (_patrolPoint == enemyBrain.transform.position)
            {
                _isWaiting = true;
                _waitTimer = enemyBrain.waitTimeAtPatrolPoint;
                return;
            }

            enemyBrain.transform.LookAt(_patrolPoint);
            
            _waitingToMove = true;
            _waitTimer = enemyBrain.waitBeforeMove; 
        }
    }
}
