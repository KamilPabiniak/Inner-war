using UnityEngine;
using UnityEngine.AI;

namespace Enemy.State
{
    public class PatrolState : IEnemyState
    {
        private Vector3 _patrolPoint;
        private bool _isWaiting;
        private float _waitTimer;
        
        //Animator
        private static readonly int CheckArea = Animator.StringToHash("Search");
        private static readonly int Speed = Animator.StringToHash("Speed");

        public void EnterState(EnemyBrain enemyBrain)
        {
            enemyBrain.audio.PlayPatrolSound();
            enemyBrain.movement.Resume();
            SetNewPatrolPoint(enemyBrain);
        }

        public void UpdateState(EnemyBrain enemyBrain)
        {
            float vel = enemyBrain.movement.agent.velocity.magnitude;
            enemyBrain.animator.SetFloat(Speed, vel); 
            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                enemyBrain.animator.SetBool(CheckArea, true); //Animator bool 
                
                if (enemyBrain.movement.IsObjectInFront())
                {
                    enemyBrain.movement.TurnTowardsFreeSpace();
                }

                if (_waitTimer <= 0f)
                {
                    _isWaiting = false;
                    enemyBrain.animator.SetBool(CheckArea, false); // Animator bool reset 
                    if (_patrolPoint != Vector3.zero)
                    {
                        if (enemyBrain.patrolAreaOverride != null)
                        {
                            enemyBrain.patrolAreaOverride.ReleasePoint(_patrolPoint);
                        }
                        else
                        {
                            EnemyPatrolHandler.ReleasePatrolPoint(_patrolPoint);
                        }
                        _patrolPoint = Vector3.zero;
                    }
                    SetNewPatrolPoint(enemyBrain);
                }
                return;
            }

            // If the agent has reached the patrol destination
            if (!enemyBrain.movement.agent.pathPending &&
                enemyBrain.movement.agent.remainingDistance <= enemyBrain.movement.agent.stoppingDistance)
            {
                // Start waiting and release current patrol point
                _isWaiting = true;
                _waitTimer = enemyBrain.waitTimeAtPatrolPoint;
            }
        }

        public void ExitState(EnemyBrain enemyBrain)
        {
            enemyBrain.animator.SetBool(CheckArea, false);
            if (!enemyBrain.movement.agent.pathPending &&
                enemyBrain.movement.agent.remainingDistance <= enemyBrain.movement.agent.stoppingDistance)
            {
                if (enemyBrain.patrolAreaOverride != null)
                {
                    enemyBrain.patrolAreaOverride.ReleasePoint(_patrolPoint);
                }
                else
                {
                    EnemyPatrolHandler.ReleasePatrolPoint(_patrolPoint);
                }
            }
            
            enemyBrain.animator.SetBool(CheckArea, false); 
        }

        private void SetNewPatrolPoint(EnemyBrain enemyBrain)
        {
            if (!enemyBrain.movement.agent.isOnNavMesh || !enemyBrain.movement.agent.enabled)
            {
                return;
            }

            _patrolPoint = enemyBrain.patrolAreaOverride != null
                ? enemyBrain.patrolAreaOverride.GetRandomPatrolPoint(enemyBrain.minPatrolPointDistance)
                : enemyBrain.RequestPatrolPoint();

            if (NavMesh.SamplePosition(_patrolPoint, out NavMeshHit hit, enemyBrain.patrolRange, NavMesh.AllAreas))
            {
                _patrolPoint = hit.position;
                if (enemyBrain.canMove)
                {
                    enemyBrain.movement.Face(_patrolPoint);
                    enemyBrain.movement.GoTo(_patrolPoint);
                }
            }
        }
    }
}
