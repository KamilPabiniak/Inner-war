using Anxiety;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.State
{
    public class AttackState : IEnemyState
    {
        private EnemyBase    _enemy;
        private NavMeshAgent _agent;
        private Vector3      _lastKnownPos;
        
        private float _attackTimer;
        private float _lostTargetTimer;
        private bool  _isOverloading;
        private float _originalSpeed;
        private const float PredictionTime = 0.5f;

        public void EnterState(EnemyBase enemy)
        {
            _enemy   = enemy;
            _agent   = enemy.navMeshAgent;
            
            _originalSpeed   = _agent.speed;
            
            _agent.speed           *= enemy.attackSpeedMultiplier;
            _agent.autoBraking      = false;
            _agent.stoppingDistance = 0f;
            _agent.updatePosition   = true;

            _attackTimer     = enemy.attackDuration;
            _lostTargetTimer = 0f;
            _isOverloading   = false;

        
            if (enemy.Target != null)
                _lastKnownPos = enemy.Target.position;

            enemy.sound.PlayAttackSound();
            enemy.SetStateChangeLock(true);
            GameEvents.onPlayerKilled += HandlePlayerKilled;
            AnxietyManager.Instance.TriggerProfileEffects();
        }

        public void UpdateState(EnemyBase enemy)
        {
            if (_isOverloading)
            {
                enemy.waitAfterAttack -= Time.deltaTime;
                if (enemy.waitAfterAttack <= 0f)
                    enemy.ChangeState(new PatrolState());
                return;
            }
            
            if (enemy.Target != null)
            {
                var predicted = enemy.Target.position;
                if (enemy.Target.TryGetComponent<Rigidbody>(out var rb))
                    predicted += rb.linearVelocity * PredictionTime;
                _lastKnownPos    = predicted;
                _lostTargetTimer = 0f;
            }
            else
            {
                _lostTargetTimer += Time.deltaTime;
                if (_lostTargetTimer >= enemy.attackAfterLostTarget)
                {
                    enemy.sound.PlayTargetLostSound();
                    enemy.ChangeState(new PatrolState());
                    return;
                }
            }
            
            var path = new NavMeshPath();
            bool pathOK = NavMesh.CalculatePath(
                enemy.transform.position,
                _lastKnownPos,
                NavMesh.AllAreas,
                path)
                && path.status == NavMeshPathStatus.PathComplete;

            if (pathOK)
                _agent.SetPath(path);
            else
                _agent.SetDestination(_lastKnownPos);
            
            if (enemy.canMove)
                _agent.Move(_agent.desiredVelocity * Time.deltaTime);
            
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                enemy.sound.PlayOverloadSound();
                _isOverloading = true;
            }
        }

        public void ExitState(EnemyBase enemy)
        {
            _agent.speed        = _originalSpeed;
            enemy.SetStateChangeLock(false);
            GameEvents.onPlayerKilled -= HandlePlayerKilled;
        }

        private void HandlePlayerKilled()
        {
            _enemy.ChangeState(new PatrolState());
        }
    }
}