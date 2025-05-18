using UnityEngine;
using UnityEngine.AI;

namespace Enemy.State
{
    public class PatrolState : IEnemyState
    {
        private Vector3 _patrolPoint;
        private bool _isWaiting;
        private float _waitTimer;

        private static readonly int CheckArea = Animator.StringToHash("Search");
        private static readonly int Speed     = Animator.StringToHash("Speed");

        public void EnterState(EnemyBrain enemyBrain)
        {
            enemyBrain.audio.PlayPatrolSound();
            enemyBrain.movement.Resume();
            _isWaiting = false;
            SetNewPatrolPoint(enemyBrain);
        }

        public void UpdateState(EnemyBrain enemyBrain)
        {
            float vel = enemyBrain.movement.agent.velocity.magnitude;
            enemyBrain.animator.SetFloat(Speed, vel * enemyBrain.animationWalkSpeedPatrol);

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

                    // Release the previous patrol point
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

            // If reached destination
            if (!enemyBrain.movement.agent.pathPending &&
                enemyBrain.movement.agent.remainingDistance <= enemyBrain.movement.agent.stoppingDistance)
            {
                // Begin waiting
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

            // If no valid point, stay and wait full duration
            if (_patrolPoint == enemyBrain.transform.position)
            {
                _isWaiting = true;
                _waitTimer = enemyBrain.waitTimeAtPatrolPoint;
                return;
            }

            // Move to the new point
            if (NavMesh.SamplePosition(_patrolPoint, out NavMeshHit hit, enemyBrain.patrolRange, NavMesh.AllAreas))
            {
                _patrolPoint = hit.position;
                if (enemyBrain.canMove)
                {
                    enemyBrain.movement.GoTo(_patrolPoint);
                }
            }
        }
    }
}
