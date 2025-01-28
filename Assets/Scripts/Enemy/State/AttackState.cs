using Enemy;
using Enemy.Type;
using UnityEngine;
using UnityEngine.AI;

public class AttackState : IEnemyState
{
    private EnemyBase _enemyBase;
    private float _attackDuration;
    private float _attackTimer;
    private float _originalSpeed;
    
    //Machine specific
    private MachineEnemy _machineEnemy;
    private bool _isOverloading = false;
    private float _lostSightTimer;

    public void EnterState(EnemyBase enemy)
    {
        if (enemy is MachineEnemy machineEnemy)
        {
            _machineEnemy = machineEnemy;
        }
        
        _enemyBase = enemy;
        enemy.sound.PlayAttackSound();
        enemy.SetStateChangeLock(true); 
        
        _attackDuration = enemy.attackDuration; 
        _attackTimer = _attackDuration;
        
        var speed = enemy.navMeshAgent.speed;
        _originalSpeed = speed;
        speed *= enemy.attackSpeedMultiplier;
        enemy.navMeshAgent.speed = speed;
        GameEvents.onPlayerKilled += HandlePlayerKilled;
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
        
        if (enemy.Target == null)
        {
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
        if (!enemy.navMeshAgent.CalculatePath(enemy.Target.position, path) || path.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning($"[{enemy.name}] Nie można wytyczyć trasy do celu. Wracam do patrolowania.");
            enemy.SetStateChangeLock(false); 
            enemy.ChangeState(new PatrolState());
            return;
        }
    
        if (enemy.canMove)
        {
            enemy.navMeshAgent.SetDestination(enemy.Target.position);
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
        enemy.navMeshAgent.speed = _originalSpeed;
        GameEvents.onPlayerKilled -= HandlePlayerKilled;
    }
    
    private void HandlePlayerKilled()
    {
        _enemyBase.ChangeState(new PatrolState());
    }
}
