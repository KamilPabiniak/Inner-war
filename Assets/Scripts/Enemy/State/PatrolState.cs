using Anxiety;
using UnityEngine;

namespace Enemy.State
{
    public class PatrolState : IEnemyState
    {
        private Vector3 _patrolPoint;
        private bool _isWaiting;                
        private bool _waitingToMove;              
        private float _waitTimer;
        private float _rotationTimer;

        private static readonly int CheckArea = Animator.StringToHash("Search");
        private static readonly int Rotation = Animator.StringToHash("Direction");

        public void EnterState(EnemyBrain enemyBrain)
        {
            enemyBrain.enemyAudio.PlayPatrolSound();
            enemyBrain.enemyMovement.Resume();
            _isWaiting = false;
            _waitingToMove = false;
            SetNewPatrolPoint(enemyBrain);
            AnxietyManager.Instance.TriggerActiveContinuous(0);
        }

        public void UpdateState(EnemyBrain enemyBrain)
        {
            if (_waitingToMove)
            {
                _waitTimer -= Time.deltaTime;
                enemyBrain.animator.SetBool(CheckArea, true);

                if (_waitTimer <= 0f)
                {
                    enemyBrain.animator.SetBool(CheckArea, false);
                    enemyBrain.animator.SetFloat(Rotation, 1f);
                    _rotationTimer -= Time.deltaTime;
                    Vector3 dir = (_patrolPoint - enemyBrain.transform.position).normalized;
                    if (dir != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(dir);
                        enemyBrain.transform.rotation = Quaternion.RotateTowards(
                            enemyBrain.transform.rotation,
                            targetRotation,
                            enemyBrain.rotationSpeed * Time.deltaTime 
                        );
                        
                        float angle = Quaternion.Angle(enemyBrain.transform.rotation, targetRotation);
                        if (angle < 1f) 
                        {
                            _waitingToMove = false;
                            enemyBrain.animator.SetFloat(Rotation, 0f);
                            if (enemyBrain.canMove)
                                enemyBrain.enemyMovement.GoTo(_patrolPoint);
                        }
                    }
                    if (_rotationTimer <= 0f)
                    {
                        FinishRotation(enemyBrain);
                        return;
                    }
                }
                return;
            }
            
            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                enemyBrain.animator.SetBool(CheckArea, true);

                if (enemyBrain.enemyMovement.IsObstacleInFront(out _))
                    enemyBrain.enemyMovement.TurnTowardsFreeSpace();

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
            
            if (!enemyBrain.enemyMovement.agent.pathPending &&
                enemyBrain.enemyMovement.agent.remainingDistance <= enemyBrain.enemyMovement.agent.stoppingDistance)
            {
                _isWaiting = true;
                _waitTimer = enemyBrain.waitTimeAtPatrolPoint;
            }
        }

        public void ExitState(EnemyBrain enemyBrain)
        {
            enemyBrain.animator.SetBool(CheckArea, false);
            enemyBrain.animator.SetFloat(Rotation, 0f);

            // Clean up current point if standing on it
            if (!enemyBrain.enemyMovement.agent.pathPending &&
                enemyBrain.enemyMovement.agent.remainingDistance <= enemyBrain.enemyMovement.agent.stoppingDistance &&
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
            if (!enemyBrain.enemyMovement.agent.isOnNavMesh || !enemyBrain.enemyMovement.agent.enabled)
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
            
            _waitingToMove = true;
            _waitTimer = enemyBrain.waitTimeAtPatrolPoint;
            _rotationTimer = enemyBrain.MaxRotationTime;
        }
        
        private void FinishRotation(EnemyBrain enemyBrain)
        {
            _waitingToMove = false;
            enemyBrain.animator.SetBool(CheckArea, false);
            enemyBrain.animator.SetFloat(Rotation, 0f);
            if (enemyBrain.canMove)
                enemyBrain.enemyMovement.GoTo(_patrolPoint);
        }
    }
}
