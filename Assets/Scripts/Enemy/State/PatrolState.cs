using System.Collections;
using System.Collections.Generic;
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
        if (enemy is not MachineEnemy machineEnemy || machineEnemy.head == null) return;

        if (_headRotationCoroutine == null)
        {
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

    private IEnumerator HeadRotationRoutine(MachineEnemy machineEnemy)
{
    float maxAngle = 45f;
    int stopPoints = Mathf.Max(2, machineEnemy.rotationStopPoints); 
    float stopDuration = machineEnemy.rotationStopDuration; 
    float rotationSpeed = Mathf.Max(0.1f, machineEnemy.headRotationSpeed); 

    List<float> rotationAngles = new List<float>();
    for (int i = 0; i < stopPoints; i++)
    {
        float angle = Mathf.Lerp(-maxAngle, maxAngle, i / (float)(stopPoints - 1)); 
        rotationAngles.Add(angle);
    }

    // Debug the rotation angles before symmetry
    Debug.Log("Initial Rotation Angles: " + string.Join(", ", rotationAngles));

    // Make the list symmetrical
    for (int i = stopPoints - 2; i >= 0; i--)
    {
        rotationAngles.Add(rotationAngles[i]);
    }

    // Debug the rotation angles after symmetry
    Debug.Log("Symmetrical Rotation Angles: " + string.Join(", ", rotationAngles));

    int currentIndex = 0;
    int direction = 1;

    // Debug to check the starting point of the loop
    Debug.Log($"Starting Rotation Loop. Initial Index: {currentIndex}, Direction: {direction}");

    // Main rotation loop
    while (isWaiting)
    {
        if (rotationAngles.Count == 0)
        {
            Debug.LogWarning("No rotation angles available! Exiting loop.");
            yield break;
        }

        float targetAngle = rotationAngles[currentIndex];
        Quaternion targetRotation = _originalHeadRotation * Quaternion.Euler(0f, 0f, targetAngle);

        // Debug the current angle and index
        Debug.Log($"Rotating to angle: {targetAngle}, Index: {currentIndex}, Direction: {direction}");

        float elapsedTime = 0f;
        Quaternion startRotation = machineEnemy.head.transform.localRotation;
        float transitionDuration = 1f / rotationSpeed;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / transitionDuration);
            machineEnemy.head.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        machineEnemy.head.transform.localRotation = targetRotation;
        yield return new WaitForSeconds(stopDuration);

        // Log for checking index and direction changes
        if (currentIndex == rotationAngles.Count - 1 && direction == 1)
        {
            direction = -1;
            Debug.Log("Reached last stop point. Changing direction to -1.");
        }
        else if (currentIndex == 0 && direction == -1)
        {
            direction = 1;
            Debug.Log("Reached first stop point. Changing direction to 1.");
        }

        currentIndex += direction;
        Debug.Log($"New Index: {currentIndex}, Direction: {direction}");
    }

    yield return null;
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

