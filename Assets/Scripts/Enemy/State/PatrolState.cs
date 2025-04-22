using UnityEngine;
using UnityEngine.AI;

namespace Enemy.State
{
    public class PatrolState : IEnemyState
    {
        private Vector3 _patrolPoint;
        private bool _isWaiting;
        private float _waitTimer;
        private static readonly int CheckArea = Animator.StringToHash("CheckArea");

        // Machine-specific head rotation

        public void EnterState(EnemyBase enemy)
        {
            enemy.sound.PlayPatrolSound();
            enemy.navMeshAgent.isStopped = false;
            SetNewPatrolPoint(enemy);
        }

        public void UpdateState(EnemyBase enemy)
        {
            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                enemy.animator.SetBool(CheckArea, true); //Animator bool
                
                if (enemy.IsObjectInFront())
                {
                    enemy.TurnTowardsFreeSpace();
                }

                if (_waitTimer <= 0f)
                {
                    _isWaiting = false;
                    
                    enemy.animator.SetBool(CheckArea, false); // Animator bool reset 
                    EnemyPatrolHandler.ReleasePatrolPoint(_patrolPoint);
                    SetNewPatrolPoint(enemy);
                }
                return;
            }

            // If the agent has reached the patrol destination
            if (!enemy.navMeshAgent.pathPending &&
                enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)
            {
                // Start waiting and release current patrol point
                _isWaiting = true;
                _waitTimer = EnemyBase.WaitTimeAtPatrolPoint;

                if (_patrolPoint != Vector3.zero)
                {
                    EnemyPatrolHandler.ReleasePatrolPoint(_patrolPoint);
                    _patrolPoint = Vector3.zero;
                }
            }
        }

        public void ExitState(EnemyBase enemy)
        {
            enemy.animator.SetBool(CheckArea, false);
            if (!enemy.navMeshAgent.pathPending &&
                enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)
            {
                EnemyPatrolHandler.ReleasePatrolPoint(_patrolPoint);
            }
        }

        private void SetNewPatrolPoint(EnemyBase enemy)
        {
            if (!enemy.navMeshAgent.isOnNavMesh || !enemy.navMeshAgent.enabled)
            {
                return;
            }

            _patrolPoint = enemy.RequestPatrolPoint();

            if (NavMesh.SamplePosition(_patrolPoint, out NavMeshHit hit, EnemyBase.PatrolRange, NavMesh.AllAreas))
            {
                _patrolPoint = hit.position;
                if (enemy.canMove)
                {
                    enemy.navMeshAgent.SetDestination(_patrolPoint);
                }
            }
        }
    }
}
