using Anxiety;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.State
{
    public class AttackState : IEnemyState
    {
        private EnemyBrain    _enemyBrain;
        private NavMeshAgent _agent;
        private Vector3      _lastKnownPos;
        
        private float _attackTimer;
        private float _lostTargetTimer;
        private bool  _isOverloading;
        private float _originalSpeed;
        private const float PredictionTime = 0.5f;

        public void EnterState(EnemyBrain enemyBrain)
        {
            _enemyBrain   = enemyBrain;
            _agent   = enemyBrain.movement.agent;
            
            _originalSpeed   = _agent.speed;
            
            _agent.speed           *= enemyBrain.attackSpeedMultiplier;
            _agent.autoBraking      = false;
            _agent.stoppingDistance = 0f;
            _agent.updatePosition   = true;

            _attackTimer     = enemyBrain.attackDuration;
            _lostTargetTimer = 0f;
            _isOverloading   = false;

        
            if (enemyBrain.Target != null)
                _lastKnownPos = enemyBrain.Target.position;

            enemyBrain.audio.PlayAttackSound();
            enemyBrain.SetStateChangeLock(true);
            GameEvents.onPlayerKilled += HandlePlayerKilled;
            AnxietyManager.Instance.TriggerProfileEffects();
        }

        public void UpdateState(EnemyBrain enemyBrain)
        {
            if (_isOverloading)
            {
                enemyBrain.waitAfterAttack -= Time.deltaTime;
                if (enemyBrain.waitAfterAttack <= 0f)
                    enemyBrain.ChangeState(new PatrolState());
                return;
            }
            
            if (enemyBrain.Target != null)
            {
                var predicted = enemyBrain.Target.position;
                if (enemyBrain.Target.TryGetComponent<Rigidbody>(out var rb))
                    predicted += rb.linearVelocity * PredictionTime;
                _lastKnownPos    = predicted;
                _lostTargetTimer = 0f;
            }
            else
            {
                _lostTargetTimer += Time.deltaTime;
                if (_lostTargetTimer >= enemyBrain.attackAfterLostTarget)
                {
                    enemyBrain.audio.PlayTargetLostSound();
                    enemyBrain.ChangeState(new PatrolState());
                    return;
                }
            }
            
            var path = new NavMeshPath();
            bool pathOK = NavMesh.CalculatePath(
                enemyBrain.transform.position,
                _lastKnownPos,
                NavMesh.AllAreas,
                path)
                && path.status == NavMeshPathStatus.PathComplete;

            if (pathOK)
                _agent.SetPath(path);
            else
                _agent.SetDestination(_lastKnownPos);
            
            if (enemyBrain.canMove)
                _agent.Move(_agent.desiredVelocity * Time.deltaTime);
            
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                enemyBrain.audio.PlayOverloadSound();
                _isOverloading = true;
            }
        }

        public void ExitState(EnemyBrain enemyBrain)
        {
            _agent.speed        = _originalSpeed;
            enemyBrain.SetStateChangeLock(false);
            GameEvents.onPlayerKilled -= HandlePlayerKilled;
        }

        private void HandlePlayerKilled()
        {
            _enemyBrain.ChangeState(new PatrolState());
        }
    }
}