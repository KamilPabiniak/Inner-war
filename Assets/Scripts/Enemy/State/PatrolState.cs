using UnityEngine;
using UnityEngine.AI;

public class PatrolState : IEnemyState
{
    private Vector3 patrolPoint;
    private bool isWaiting;
    private float waitTimer;
    public void EnterState(EnemyBase enemy)
    {
        Debug.Log($"[{enemy.name}] Wchodzi w stan patrolowania.");
        SetNewPatrolPoint(enemy);
    }

    public void UpdateState(EnemyBase enemy)
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                SetNewPatrolPoint(enemy);
            }
            return;
        }

        if (!enemy.navMeshAgent.pathPending && enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)
        {
            isWaiting = true;
            waitTimer = enemy.waitTimeAtPatrolPoint;
            enemy.ReleasePatrolPoint(patrolPoint);
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        if (!(enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)) return;
        Debug.Log($"[{enemy.name}] Opuszcza stan patrolowania.");
        enemy.ReleasePatrolPoint(patrolPoint);
    }
    
    private void SetNewPatrolPoint(EnemyBase enemy)
    {
        if (!enemy.navMeshAgent.isOnNavMesh || !enemy.navMeshAgent.enabled) 
        {
            Debug.LogError($"[{enemy.name}] Agent nie jest na NavMesh!");
            return;
        }
        patrolPoint = enemy.RequestPatrolPoint();
        
        if (NavMesh.SamplePosition(patrolPoint, out NavMeshHit hit, enemy.patrolRange, NavMesh.AllAreas))
        {
            if (enemy.navMeshAgent.pathPending || enemy.navMeshAgent.remainingDistance > enemy.navMeshAgent.stoppingDistance)
                return; 
            patrolPoint = hit.position;
            enemy.navMeshAgent.SetDestination(patrolPoint);
            Debug.Log($"[{enemy.name}] Nowy punkt patrolowy: {patrolPoint}.");
        }
        else
        {
            Debug.LogWarning($"[{enemy.name}] Nie uda�o si� znale�� punktu na NavMesh w okolicy: {patrolPoint}");
        }
    }
}