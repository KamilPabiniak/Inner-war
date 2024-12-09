using UnityEngine;
using UnityEngine.AI;

public class InvestigateState : IEnemyState
{
    private Vector3 lastKnownPosition;
    private float investigationTimer;
    private bool isWaiting;
    private float waitTime;

    public InvestigateState(Vector3 position)
    {
        lastKnownPosition  = position;
    }

    public void EnterState(EnemyBase enemy)
    {
        Debug.Log($"[{enemy.name}] Idê sprawdziæ miejsce alarmu.");
        waitTime = enemy.waitTimeAtInvestigation;
        investigationTimer = 0f;
        isWaiting = false;
        
        if (NavMesh.SamplePosition(lastKnownPosition, out NavMeshHit hit, enemy.patrolRange, NavMesh.AllAreas))
        {
            enemy.navMeshAgent.SetDestination(hit.position);
            Debug.Log($"[{enemy.name}] Idê do ostatniej znanej pozycji gracza: {lastKnownPosition}");
        }
        else
        {
            Debug.LogWarning($"[{enemy.name}] Nie mogê znaleŸæ pozycji na NavMesh w okolicy {lastKnownPosition}");
            enemy.ChangeState(new PatrolState());
        }
    }

    public void UpdateState(EnemyBase enemy)
    {
        Transform target = enemy.target;

        if (target != null && CanSeeTarget(enemy, target))
        {
            lastKnownPosition = target.position;

            if (NavMesh.SamplePosition(lastKnownPosition, out NavMeshHit hit, enemy.patrolRange, NavMesh.AllAreas))
            {
                enemy.navMeshAgent.SetDestination(hit.position);
                Debug.Log($"[{enemy.name}] Widzê gracza! Pod¹¿am za nim.");
            }
        }
        else if (!enemy.navMeshAgent.pathPending && enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                investigationTimer = waitTime;
                Debug.Log($"[{enemy.name}] Nie widzê gracza. Czekam w miejscu: {lastKnownPosition}");
            }
            else
            {
                investigationTimer -= Time.deltaTime;

                if (investigationTimer <= 0f)
                {
                    Debug.Log($"[{enemy.name}] Nie znalaz³em gracza. Wracam do patrolowania.");
                    enemy.ChangeState(new PatrolState());
                }
            }
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        Debug.Log($"[{enemy.name}] Opuszcza stan sprawdzania alarmu.");
    }
    
    private bool CanSeeTarget(EnemyBase enemy, Transform target)
    {
        Vector3 directionToTarget = (target.position - enemy.transform.position).normalized;
        float distanceToTarget = Vector3.Distance(enemy.transform.position, target.position);

        if (Physics.Raycast(enemy.transform.position, directionToTarget, out RaycastHit hit, distanceToTarget))
        {
            if (hit.transform == target)
            {
                return true; 
            }
        }
        return false; 
    }
}