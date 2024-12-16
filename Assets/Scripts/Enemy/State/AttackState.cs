using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AttackState : IEnemyState
{
    private EnemyBase _enemyBase;
    private float _attackDuration;
    private float _attackTimer;
    private float _originalSpeed;
    private Coroutine _headRotationCoroutine;
    private Quaternion _originalHeadRotation;
    private Transform _target;

    public AttackState()
    {
    }

    public void EnterState(EnemyBase enemy)
    {
        Debug.Log($"[{enemy.name}] Wchodzê w stan ataku!");

        _target = enemy.target;
        _enemyBase = enemy;
        _attackDuration = enemy.attackDuration; 
        _attackTimer = _attackDuration;
        _originalSpeed = enemy.navMeshAgent.speed;
        enemy.navMeshAgent.speed *= enemy.attackSpeedMultiplier; 

        SaveOriginalHeadRotation(enemy);
    }

    public void UpdateState(EnemyBase enemy)
    {
        if (_target == null)
        {
            Debug.LogWarning($"[{enemy.name}] Utracono cel jakimœ cudem. Wracam do patrolowania.");
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
            Debug.LogWarning($"[{enemy.name}] Nie mo¿na wytyczyæ trasy do celu. Wracam do patrolowania.");
            enemy.ChangeState(new PatrolState());
            return;
        }
        
        if (!_target)
        {
            enemy.navMeshAgent.SetDestination(_target.position);
        }
        
        StartHeadRotation(enemy, _target.position);
        
        if (enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)
        {
            PlayerDeath playerDeath = _target.GetComponent<PlayerDeath>();
            if (playerDeath != null)
            {
                playerDeath.Kill();
            }

            enemy.ChangeState(new PatrolState());
            return;
        }
        
        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0f)
        {
            Debug.Log($"[{enemy.name}] Czas ataku min¹³. Wracam do patrolowania.");
            enemy.ChangeState(new PatrolState());
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        ResetHeadRotation(enemy);
        enemy.navMeshAgent.speed = _originalSpeed;
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
        float maxAngle = 45f;

        while (true)
        {
            Vector3 directionToTarget = (targetPosition - headTransform.position).normalized;

            if (directionToTarget.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.up, -directionToTarget);
                Quaternion clampedRotation = ClampRotationToMaxAngle(machineEnemy, targetRotation, maxAngle);

                headTransform.rotation = Quaternion.Slerp(
                    headTransform.rotation,
                    clampedRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            Debug.DrawRay(headTransform.position, directionToTarget * 2f, Color.red);
            yield return null;
        }
    }

    private Quaternion ClampRotationToMaxAngle(MachineEnemy machineEnemy, Quaternion targetRotation, float maxAngle)
    {
        Quaternion clampedRotation = Quaternion.RotateTowards(
            _originalHeadRotation,
            targetRotation,
            maxAngle
        );

        return clampedRotation;
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
