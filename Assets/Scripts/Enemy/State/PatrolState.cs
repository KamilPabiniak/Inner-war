using System.Collections;
using System.Collections.Generic;
using Enemy;
using Enemy.Type;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : IEnemyState
{
    private Vector3 _patrolPoint;
    private bool _isWaiting;
    private float _waitTimer;

    // Machine-specific
    private Coroutine _headRotationCoroutine;
    private int _lastHeadPositionIndex;

    public void EnterState(EnemyBase enemy)
    {
        enemy.sound.PlayPatrolSound();
        enemy.navMeshAgent.isStopped = false;
        SetNewPatrolPoint(enemy);
    }

    public void UpdateState(EnemyBase enemy)
    {
        if (_isWaiting)
        {
            _waitTimer -= Time.deltaTime;
            StartHeadRotation(enemy);

            if (!(_waitTimer <= 0f)) return;
            _isWaiting = false;
            ResetHeadRotation(enemy);
            SetNewPatrolPoint(enemy);
            return;
        }

        if (!enemy.navMeshAgent.pathPending &&
            enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)
        {
            _isWaiting = true;
            _waitTimer = enemy.waitTimeAtPatrolPoint;
            if (_patrolPoint != Vector3.zero)
            {
                EnemyPatrolHandler.ReleasePatrolPoint(_patrolPoint);
                _patrolPoint = Vector3.zero;
            }
        }
        
        //In test
        if (IsObjectInFront(enemy))
        {
            TurnAwayFromEdge(enemy);
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        if (!(enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)) return;
        EnemyPatrolHandler.ReleasePatrolPoint(_patrolPoint);
        ResetHeadRotation(enemy);
    }

    private void SetNewPatrolPoint(EnemyBase enemy)
    {
        if (!enemy.navMeshAgent.isOnNavMesh || !enemy.navMeshAgent.enabled)
        {
            Debug.LogError($"[{enemy.name}] Agent nie jest na NavMesh!");
            return;
        }

        _patrolPoint = enemy.RequestPatrolPoint();

        if (NavMesh.SamplePosition(_patrolPoint, out NavMeshHit hit, enemy.patrolRange, NavMesh.AllAreas))
        {
            if (enemy.navMeshAgent.pathPending ||
                enemy.navMeshAgent.remainingDistance > enemy.navMeshAgent.stoppingDistance)
                return;
            _patrolPoint = hit.position;
            if (!enemy.canMove) return;
            enemy.navMeshAgent.SetDestination(_patrolPoint);
        }
        else
        {
            Debug.LogWarning($"[{enemy.name}] Nie uda�o si� znale�� punktu na NavMesh w okolicy: {_patrolPoint}");
        }
    }
    
    private bool IsObjectInFront(EnemyBase enemy)
    {
        RaycastHit hit;
        Vector3 startPosition = enemy.transform.position;
        Vector3 direction = enemy.transform.rotation * Vector3.forward; 
        
        Debug.DrawLine(startPosition, startPosition + direction * 2f, Color.magenta, 1f);
        if (Physics.Raycast(startPosition, direction, out hit, 2f))
        {
            return true;
        }
        
        return false;
    }
    
    private void TurnAwayFromEdge(EnemyBase enemy)
    {
        Vector3 currentForward = enemy.transform.forward;
        Vector3 oppositeDirection = -currentForward;

        // Ustawiamy nowy kierunek dla agenta
        Quaternion targetRotation = Quaternion.LookRotation(oppositeDirection);
        enemy.transform.rotation = Quaternion.RotateTowards(enemy.transform.rotation, targetRotation, enemy.rotationMultiplier * Time.deltaTime);
    }

    private void StartHeadRotation(EnemyBase enemy)
    {
        if (enemy is not MachineEnemy machineEnemy) return;

        if (_headRotationCoroutine == null)
        {
            _headRotationCoroutine = enemy.StartCoroutine(HeadRotationRoutine(machineEnemy));
        }
    }
    
    private IEnumerator HeadRotationRoutine(MachineEnemy machineEnemy)
    {
        float maxDistance = machineEnemy.maxOffsetDistance;
        int stopPoints = machineEnemy.stopPoints;
        float stopDuration = machineEnemy.stopDuration;
        float headRotationSpeed = machineEnemy.headRotationSpeed;

        List<Vector3> movementOffsets = new List<Vector3>();
        for (int i = 0; i < stopPoints; i++)
        {
            float offset = Mathf.Lerp(-maxDistance, maxDistance, i / (float)(stopPoints - 1));
            movementOffsets.Add(new Vector3(offset, 0f, 0f)); 
        }

        for (int i = stopPoints - 2; i >= 0; i--)
        {
            movementOffsets.Add(movementOffsets[i]);
        }

        int direction = 1;

        yield return new WaitForSeconds(1f);

        while (_isWaiting)
        {
            if (movementOffsets.Count == 0)
            {
                Debug.LogWarning("No rotation angles available! Exiting loop.");
                yield break;
            }

            Vector3 targetLocalPosition = machineEnemy.OriginalHeadPos + movementOffsets[_lastHeadPositionIndex];
            float elapsedTime = 0f;
            Vector3 startLocalPosition = machineEnemy.sightTarget.transform.localPosition;
            float transitionDuration = headRotationSpeed;

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / transitionDuration);
                machineEnemy.sightTarget.transform.localPosition =
                    Vector3.Lerp(startLocalPosition, targetLocalPosition, t);
                yield return null;
            }

            machineEnemy.sightTarget.transform.localPosition = targetLocalPosition;
            yield return new WaitForSeconds(stopDuration);

            if (_lastHeadPositionIndex == movementOffsets.Count - 1 && direction == 1)
            {
                direction = -1;
            }
            else if (_lastHeadPositionIndex == 0 && direction == -1)
            {
                direction = 1;
            }

            _lastHeadPositionIndex += direction;
        }
    }


    private void ResetHeadRotation(EnemyBase enemy)
    {
        if (enemy is MachineEnemy machineEnemy)
        {
            if (_headRotationCoroutine != null)
            {
                enemy.StopCoroutine(_headRotationCoroutine);
                _headRotationCoroutine = null;
            }

            enemy.StartCoroutine(SmoothResetPosition(machineEnemy));
        }
    }

    private IEnumerator SmoothResetPosition(MachineEnemy machineEnemy)
    {
        float duration = machineEnemy.headRotationSpeed;
        Vector3 startPosition = machineEnemy.sightTarget.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
            machineEnemy.sightTarget.transform.localPosition =
                Vector3.Lerp(startPosition, machineEnemy.OriginalHeadPos, t);
            yield return null;
        }

        machineEnemy.sightTarget.transform.localPosition = machineEnemy.OriginalHeadPos;
    }
}

