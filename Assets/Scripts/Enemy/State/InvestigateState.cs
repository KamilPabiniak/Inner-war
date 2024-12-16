using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class InvestigateState : IEnemyState
{
    private EnemyBase _enemyBase;
    private Vector3 _lastKnownPosition;
    private float _investigationTimer;
    private bool _isWaiting;
    private float _waitTime;
    private Coroutine _headRotationCoroutine;
    private Quaternion _originalHeadRotation;
    private Transform _debugTarget;

    public InvestigateState(Vector3 position, EnemyBase enemyBase)
    {
        _lastKnownPosition  = position;
        _enemyBase = enemyBase;
    }

    public void EnterState(EnemyBase enemy)
    {
        Debug.Log($"[{enemy.name}] Idę sprawdzić podejrzany ruch.");
        _waitTime = enemy.waitTimeAtInvestigation;
        _investigationTimer = 0f;
        _isWaiting = false;
        SaveOriginalHeadRotation(enemy);

        if (NavMesh.SamplePosition(_lastKnownPosition, out NavMeshHit hit, enemy.patrolRange, NavMesh.AllAreas))
        {
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
            _lastKnownPosition = target.position;

            if (NavMesh.SamplePosition(_lastKnownPosition, out NavMeshHit hit, enemy.patrolRange, NavMesh.AllAreas))
            {
                enemy.navMeshAgent.SetDestination(hit.position);
                StartHeadRotation(enemy, hit.position);
            }
        }
        else
        {
            _enemyBase.ClearTarget();
            ResetHeadRotation(enemy);
        }

        if (enemy.navMeshAgent.pathPending || !(enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)) return;
        if (!_isWaiting)
        {
            _isWaiting = true;
            _investigationTimer = _waitTime;
            _enemyBase.ClearTarget();
        }
        else
        {
            _investigationTimer -= Time.deltaTime;

            if (_investigationTimer <= 0f)
            {
                _enemyBase.ClearTarget();
                enemy.ChangeState(new PatrolState());
            }
        }
    }


    public void ExitState(EnemyBase enemy)
    {
        ResetHeadRotation(enemy);
    }
    
    
    private void SaveOriginalHeadRotation(EnemyBase enemy)
    {
        if (enemy is MachineEnemy machineEnemy && machineEnemy.head != null)
        {
            _originalHeadRotation = machineEnemy.originalHeadRot;
        }
    }

    private void StartHeadRotation(EnemyBase enemy, Vector3 targetPosition)
    {
        if (enemy is MachineEnemy machineEnemy && machineEnemy.head != null)
        {
            targetPosition.y += 1.5f; 
            
            if (_headRotationCoroutine != null)
            {
                enemy.StopCoroutine(_headRotationCoroutine);
            }
            
            _headRotationCoroutine = enemy.StartCoroutine(RotateHeadTowards(machineEnemy, targetPosition));
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


    private IEnumerator RotateHeadTowards(MachineEnemy machineEnemy, Vector3 targetPosition)
    {
        float rotationSpeed = machineEnemy.headRotationSpeed;
        Transform headTransform = machineEnemy.head.transform;

        while (true)
        {
            Vector3 directionToTarget = (targetPosition - headTransform.position).normalized;

            if (directionToTarget.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.up, -directionToTarget);
                
                headTransform.rotation = Quaternion.Slerp(
                    headTransform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            Debug.DrawRay(directionToTarget,  Vector3.up * 2f, Color.cyan);
            yield return null;
        }
    }


    private IEnumerator SmoothResetHeadRotation(MachineEnemy machineEnemy)
    {
        float duration = 1f;
        Quaternion startRotation = machineEnemy.head.transform.localRotation;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            machineEnemy.head.transform.localRotation = Quaternion.Slerp(startRotation, _originalHeadRotation, t);
            yield return null;
        }

        machineEnemy.head.transform.localRotation = _originalHeadRotation;
    }

}