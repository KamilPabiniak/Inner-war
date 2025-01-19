using UnityEngine;
using UnityEngine.AI;

public class AttackState : IEnemyState
{
    private EnemyBase _enemyBase;
    private float _attackDuration;
    private float _attackTimer;
    private float _originalSpeed;
    private Transform _target;
    
    //Machine specific
    private MachineEnemy _machineEnemy;
    private float _overloadTimer; 
    private bool _isOverloading = false;
    private float _lostSightTimer;

    public void EnterState(EnemyBase enemy)
    {
        if (enemy is MachineEnemy machineEnemy)
        {
            _machineEnemy = machineEnemy;
        }
        enemy.sound.PlayAttackSound();
        enemy.SetStateChangeLock(true); 
        _target = enemy.Target;
        _enemyBase = enemy;
        _attackDuration = enemy.attackDuration; 
        _attackTimer = _attackDuration;
        _originalSpeed = enemy.navMeshAgent.speed;
        enemy.navMeshAgent.speed *= enemy.attackSpeedMultiplier;
        _overloadTimer = _machineEnemy.overloadTimer;
        enemy.OnPlayerKilled += HandlePlayerKilled;
    }

    public void UpdateState(EnemyBase enemy)
    {
        if (_isOverloading)
        {
            _machineEnemy.overloadTimer -= Time.deltaTime;
            if (_machineEnemy.overloadTimer <= 0f)
            {
                Debug.Log($"[{enemy.name}] Przeciążenie zakończone. Wracam do patrolowania.");
                enemy.ChangeState(new PatrolState());
            }
            return;
        }
        
        if (_target == null)
        {
            Debug.LogWarning(_target);
            _lostSightTimer += Time.deltaTime;
            if (_lostSightTimer >= enemy.maxInvestigationTimeAfterLoseSight / 2)
            {
                enemy.sound.PlayTargetLostSound();
                enemy.ChangeState(new PatrolState());
                return;
            }
        }
        else
        {
            _lostSightTimer = 0f; 
        }
        
        NavMeshPath path = new NavMeshPath();
        if (!enemy.navMeshAgent.CalculatePath(_target.position, path) || path.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning($"[{enemy.name}] Nie można wytyczyć trasy do celu. Wracam do patrolowania.");
            enemy.SetStateChangeLock(false); 
            enemy.ChangeState(new PatrolState());
            return;
        }
    
        if (enemy.canMove)
        {
            enemy.navMeshAgent.SetDestination(_target.position);
        }
        
        _attackTimer -= Time.deltaTime;
        if (!(_attackTimer <= 0f)) return;
        enemy.sound.PlayOverloadSound();
        enemy.navMeshAgent.isStopped = true; 
        _isOverloading = true;
    }

    public void ExitState(EnemyBase enemy)
    {
        enemy.SetStateChangeLock(false); 
        _enemyBase.ClearTarget();
        enemy.navMeshAgent.speed = _originalSpeed;
        enemy.OnPlayerKilled -= HandlePlayerKilled;
    }
    
    
    private void HandlePlayerKilled()
    {
        _enemyBase.ChangeState(new PatrolState());
    }
}
