using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

public class AttackState : IEnemyState
{
    private EnemyBase _enemyBase;
    private float _attackDuration;
    private float _attackTimer;
    private float _originalSpeed;
    private Transform _target;

    public void EnterState(EnemyBase enemy)
    {
        enemy.SetStateChangeLock(true); 
        _target = enemy.target;
        _enemyBase = enemy;
        _attackDuration = enemy.attackDuration; 
        _attackTimer = _attackDuration;
        _originalSpeed = enemy.navMeshAgent.speed;
        enemy.navMeshAgent.speed *= enemy.attackSpeedMultiplier; 
    }

    public void UpdateState(EnemyBase enemy)
    {
        if (_target == null)
        {
            Debug.LogWarning($"[{enemy.name}] Utracono cel jakimś cudem. Wracam do patrolowania.");
            enemy.ChangeState(new PatrolState());
            return;
        }
        
        if (enemy is MachineEnemy)
        {
            if (_target != null)
            {
                //EnemyMediator.SendAlert(_target.position);
            }
        }

        NavMeshPath path = new NavMeshPath();
        if (!enemy.navMeshAgent.CalculatePath(_target.position, path) || path.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning($"[{enemy.name}] Nie można wytyczyć trasy do celu. Wracam do patrolowania.");
            enemy.ChangeState(new PatrolState());
            return;
        }
    
        if (enemy.canMove)
        {
            enemy.navMeshAgent.SetDestination(_target.position);
        }

        if (Vector3.Distance(_target.position, enemy.transform.position) <= enemy.navMeshAgent.stoppingDistance)
        {
            if (enemy.canKill)
            {
                PlayerDeath playerDeath = _target.GetComponent<PlayerDeath>();
                if (playerDeath != null)
                {
                    playerDeath.Kill();
                }
            }

            enemy.ChangeState(new PatrolState());
            return;
        }
        
        
        _attackTimer -= Time.deltaTime;
        if (!(_attackTimer <= 0f)) return;
        Debug.Log($"[{enemy.name}] Przeciążenie ogniw. Resetuje.");
        enemy.ChangeState(new PatrolState());
    }

    public void ExitState(EnemyBase enemy)
    {
        enemy.SetStateChangeLock(false); 
        _enemyBase.ClearTarget();
        enemy.navMeshAgent.speed = _originalSpeed;
    }

   
}
