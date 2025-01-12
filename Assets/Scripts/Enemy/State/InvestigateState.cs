using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class InvestigateState : IEnemyState
{
    private readonly EnemyBase _enemyBase;
    private Vector3 _lastKnownPosition;
    private Coroutine _headRotationCoroutine;
    private float _lostSightTimer;

    public InvestigateState(Vector3 position, EnemyBase enemyBase)
    {
        _lastKnownPosition  = position;
        _enemyBase = enemyBase;
    }

    public void EnterState(EnemyBase enemy)
    {
        enemy.SetStateChangeLock(true);
        if (NavMesh.SamplePosition(_lastKnownPosition, out NavMeshHit hit, enemy.patrolRange, NavMesh.AllAreas))
        {
            if (!enemy.canMove) return;
            enemy.navMeshAgent.SetDestination(hit.position);
        }
        else
        {
            enemy.ChangeState(new PatrolState());
        }
    }

    public void UpdateState(EnemyBase enemy)
    {
        Transform target = enemy.target;
        if (target != null && enemy.CanSeeTarget())
        {
            _lostSightTimer = 0f;
            _lastKnownPosition = target.position;

            if (NavMesh.SamplePosition(_lastKnownPosition, out NavMeshHit hit, enemy.patrolRange, NavMesh.AllAreas))
            {
                if (enemy.canMove)
                {
                    enemy.navMeshAgent.SetDestination(hit.position);
                }
                RotateHeadTowards(target);
            }
        }
        else
        {
            Debug.Log($"[{enemy.name}] Cel zgubiony. Timer: {_lostSightTimer:F2}");
            _lostSightTimer += Time.deltaTime;
            
            if (_lostSightTimer < enemy.maxInvestigationTimeAfterLoseSight)
            {
                if (NavMesh.SamplePosition(_lastKnownPosition, out NavMeshHit hit, enemy.patrolRange, NavMesh.AllAreas))
                {
                    if (enemy.canMove)
                    {
                        enemy.navMeshAgent.SetDestination(hit.position);
                        ResetHeadRotation();
                        Debug.Log($"[{enemy.name}] Kontynuujê poszukiwania w ostatniej znanej pozycji: {hit.position}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[{enemy.name}] Cel zgubiony na dobre. Powrót do patrolu.");
                enemy.ChangeState(new PatrolState());
            }
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        enemy.SetStateChangeLock(false); 
        ResetHeadRotation();
        _enemyBase.ClearTarget();
    }
    
    private void RotateHeadTowards(Transform target)
{
    if (_enemyBase is not MachineEnemy machineEnemy || machineEnemy.head == null) return;
    
    Vector3 directionToTarget = target.position - machineEnemy.head.transform.position;
    
    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
    

    if (_headRotationCoroutine != null)
    {
        _enemyBase.StopCoroutine(_headRotationCoroutine);
    }
    _headRotationCoroutine = _enemyBase.StartCoroutine(SmoothRotateHead(machineEnemy, targetRotation));
}

private IEnumerator SmoothRotateHead(MachineEnemy machineEnemy, Quaternion targetRotation)
{
    float duration = 0.5f; 
    Quaternion startRotation = machineEnemy.head.transform.localRotation;
    float elapsedTime = 0f;

    while (elapsedTime < duration)
    {
        elapsedTime += Time.deltaTime;
        float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
        machineEnemy.head.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
        yield return null;
    }

    machineEnemy.head.transform.localRotation = targetRotation;
}

private void ResetHeadRotation()
{
    if (_enemyBase is MachineEnemy machineEnemy && machineEnemy.head != null)
    {
        if (_headRotationCoroutine != null)
        {
            _enemyBase.StopCoroutine(_headRotationCoroutine);
        }
    }
}

    


}