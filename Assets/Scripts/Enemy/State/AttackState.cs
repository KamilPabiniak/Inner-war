using Anxiety;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.State
{
    public class AttackState : IEnemyState
    {
        private EnemyBrain   _enemyBrain;
        private NavMeshAgent _agent;
        private Vector3      _lastKnownPos;
        
        private float _attackTimer;
        private float _lostTargetTimer;
        private bool  _isOverloading;
        private float _originalSpeed;
        private float _originalAngularSpeed;
        private const float PredictionTime = 0.5f;
        private bool _escapeSoundPlayed;

        public void EnterState(EnemyBrain enemyBrain)
        {
            _enemyBrain            = enemyBrain;
            _agent                 = enemyBrain.movement.agent;

            // Cache original values
            _originalSpeed         = _agent.speed;
            _originalAngularSpeed  = _agent.angularSpeed;

            // Configure for aggressive pursuit
            _agent.speed           = _originalSpeed * enemyBrain.attackSpeedMultiplier;
            _agent.angularSpeed    = 360f;                 
            _agent.autoBraking     = false;                
            _agent.stoppingDistance= 0f;
            _agent.updatePosition  = true;
            _agent.updateRotation  = true;
            _agent.isStopped       = false;

            // Reset timers
            _attackTimer           = enemyBrain.attackLockDuration;
            _lostTargetTimer       = 0f;
            _isOverloading         = false;

            if (enemyBrain.Target != null)
                _lastKnownPos      = enemyBrain.Target.position;

            AnxietyManager.Instance.IncreaseFear(5f);
            // Play effects
            enemyBrain.audio.PlayAttackSound();
            GameEvents.onPlayerKilled += HandlePlayerKilled;
            AnxietyManager.Instance.TriggerProfileEffects();
        }

        public void UpdateState(EnemyBrain enemyBrain)
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                enemyBrain.audio.PlayOverloadSound();
                _isOverloading = true;
            }
            
            if (_isOverloading)
            {
                enemyBrain.waitAfterOverload -= Time.deltaTime;
                if (enemyBrain.waitAfterOverload <= 0f)
                {
                    enemyBrain.detection.SetAwarenessLevel(0f);
                    enemyBrain.OnBackToPatrol(); 
                }
                return;
            }
            
            if (enemyBrain.detection.IsPlayerVisible && !enemyBrain.IsTargetInNavMesh(out _))
            {
                if (!_escapeSoundPlayed)
                {
                    enemyBrain.audio.PlayTargetEscapeSound();
                    _escapeSoundPlayed = true;
                }
                
                enemyBrain.detection.SetAwarenessLevel(0f);
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
                    return;
                }
            }

            if (enemyBrain.canMove)
            {
                _agent.isStopped = false;
               enemyBrain.movement.GoTo(_lastKnownPos);
            }
            else
            {
                _agent.isStopped = true;
            }
        }

        public void ExitState(EnemyBrain enemyBrain)
        {
            _agent.isStopped        = true;
            _agent.speed            = _originalSpeed;
            _agent.angularSpeed     = _originalAngularSpeed;
            _agent.autoBraking      = true;
            _agent.stoppingDistance = _originalAngularSpeed; 

            GameEvents.onPlayerKilled -= HandlePlayerKilled;
        }

        private void HandlePlayerKilled() =>
            _enemyBrain.detection.SetAwarenessLevel(0f);
    }
}