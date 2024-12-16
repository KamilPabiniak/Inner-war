using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : IEnemyState
{
    private Vector3 patrolPoint;
    private bool isWaiting;
    private float waitTimer;
    
    // Machine-specific
    private Quaternion _originalHeadRotation; 
    private Coroutine _headRotationCoroutine; 
    
    public void EnterState(EnemyBase enemy)
    {
        Debug.Log($"[{enemy.name}] Wchodzi w stan patrolowania.");
        SaveOriginalHeadRotation(enemy);
        SetNewPatrolPoint(enemy);
    }

    public void UpdateState(EnemyBase enemy)
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            StartHeadRotation(enemy);
            
            if (!(waitTimer <= 0f)) return;
            isWaiting = false;
            ResetHeadRotation(enemy);
            SetNewPatrolPoint(enemy);
            return;
        }

        if (!enemy.navMeshAgent.pathPending && enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)
        {
            isWaiting = true;
            waitTimer = enemy.waitTimeAtPatrolPoint;
            if (patrolPoint != Vector3.zero)
            {
                EnemyMediator.ReleasePatrolPoint(patrolPoint);
                patrolPoint = Vector3.zero; 
            }
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        if (!(enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)) return;
        EnemyMediator.ReleasePatrolPoint(patrolPoint);
        ResetHeadRotation(enemy);
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
        }
        else
        {
            Debug.LogWarning($"[{enemy.name}] Nie uda�o si� znale�� punktu na NavMesh w okolicy: {patrolPoint}");
        }
    }
    
     private void SaveOriginalHeadRotation(EnemyBase enemy)
    {
        if (enemy is MachineEnemy machineEnemy && machineEnemy.head != null)
        {
            _originalHeadRotation = machineEnemy.originalHeadRot;
        }
    }

    private void StartHeadRotation(EnemyBase enemy)
    {
        if (enemy is MachineEnemy machineEnemy && machineEnemy.head != null)
        {
            if (_headRotationCoroutine != null)
            {
                enemy.StopCoroutine(_headRotationCoroutine);
            }
            _headRotationCoroutine = enemy.StartCoroutine(HeadRotationRoutine(machineEnemy));
        }
    }

    private void ResetHeadRotation(EnemyBase enemy)
    {
        if (enemy is MachineEnemy machineEnemy && machineEnemy.head != null)
        {
            if (_headRotationCoroutine != null)
            {
                enemy.StopCoroutine(_headRotationCoroutine);
                _headRotationCoroutine = null;
            }
            
            enemy.StartCoroutine(SmoothResetHeadRotation(machineEnemy));
        }
    }
    
    private float oscillationTime = 0f; 

    private IEnumerator HeadRotationRoutine(MachineEnemy machineEnemy)
    {
        float maxAngle = 45f;
        float rotationSpeed = machineEnemy.headRotationSpeed; 

        while (isWaiting)
        {
            float currentAngle = maxAngle * Mathf.Sin(2 * Mathf.PI * rotationSpeed * oscillationTime); 
            machineEnemy.head.transform.localRotation = _originalHeadRotation * Quaternion.Euler(0f, 0f, currentAngle);
           
            oscillationTime += Time.deltaTime;
            yield return null; 
        }
        
        oscillationTime = 0f;
    }




    private IEnumerator SmoothResetHeadRotation(MachineEnemy machineEnemy)
    {
        float duration = 1f; 
        Quaternion startRotation = machineEnemy.head.transform.localRotation;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
            machineEnemy.head.transform.localRotation = Quaternion.Slerp(startRotation, _originalHeadRotation, t);
            yield return null;
        }

        machineEnemy.head.transform.localRotation = _originalHeadRotation;
    }

}