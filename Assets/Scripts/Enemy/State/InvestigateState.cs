using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class InvestigateState : IEnemyState
{
    private EnemyBase _enemyBase;
    private Vector3 _lastKnownPosition;
    private Coroutine _headRotationCoroutine;
    private float _lostSightTimer;

    public InvestigateState(Vector3 position)
    {
        _lastKnownPosition  = position;
    }

    public void EnterState(EnemyBase enemy)
    {
        _enemyBase = enemy;
        enemy.sound.PlayInvestigateSound();
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
        Transform target = enemy.Target;
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
                if (enemy is MachineEnemy machineEnemy)
                {
                    TrackTarget(machineEnemy, target);
                }
            }
        }
        else
        {
            //Debug.Log($"[{enemy.name}] Cel zgubiony. Timer: {_lostSightTimer:F2}");
            _lostSightTimer += Time.deltaTime;
            
            if (_lostSightTimer < enemy.maxInvestigationTimeAfterLoseSight)
            {
                if (NavMesh.SamplePosition(_lastKnownPosition, out NavMeshHit hit, enemy.patrolRange, NavMesh.AllAreas))
                {
                    if (enemy.canMove)
                    {
                        enemy.navMeshAgent.SetDestination(hit.position);
                        if (enemy is MachineEnemy machineEnemy)
                        {
                            ResetSightTargetPosition(machineEnemy);
                        }
                        //Debug.Log($"[{enemy.name}] Kontynuujê poszukiwania w ostatniej znanej pozycji: {hit.position}");
                    }
                }
            }
            else
            {
                //Debug.LogWarning($"[{enemy.name}] Cel zgubiony na dobre. Powrót do patrolu.");
                enemy.ChangeState(new PatrolState());
            }
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        enemy.SetStateChangeLock(false);
        if (enemy is MachineEnemy machineEnemy)
        {
            ResetSightTargetPosition(machineEnemy);
        }
    }
    
  private void TrackTarget(MachineEnemy enemy, Transform target)
    {
        if (_headRotationCoroutine != null)
        {
            _enemyBase.StopCoroutine(_headRotationCoroutine);
        }

        _headRotationCoroutine = _enemyBase.StartCoroutine(MoveSightToTarget(enemy, target.position));
    }

    private IEnumerator MoveSightToTarget(MachineEnemy enemy, Vector3 targetPosition)
    {
        Vector3 originalLocalPosition = enemy.sightTarget.transform.localPosition;
        Vector3 targetLocalOffset = enemy.transform.InverseTransformPoint(targetPosition) - originalLocalPosition;
        
        targetLocalOffset = Vector3.ClampMagnitude(targetLocalOffset, enemy.maxOffsetDistance);

        float elapsedTime = 0f;

        while (elapsedTime < enemy.headRotationSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / enemy.headRotationSpeed);
            enemy.sightTarget.transform.localPosition =
                Vector3.Lerp(originalLocalPosition, originalLocalPosition + targetLocalOffset, t);
            yield return null;
        }

        enemy.sightTarget.transform.localPosition = originalLocalPosition + targetLocalOffset;
    }

    private void ResetSightTargetPosition(MachineEnemy enemy)
    {
        if (_headRotationCoroutine != null)
        {
            _enemyBase.StopCoroutine(_headRotationCoroutine);
            _headRotationCoroutine = null;
        }

        enemy.StartCoroutine(SmoothResetSightTarget(enemy));
    }

    private IEnumerator SmoothResetSightTarget(MachineEnemy enemy)
    {
        Vector3 startPosition = enemy.sightTarget.transform.localPosition;
        Vector3 resetPosition = enemy.OriginalHeadPos; 
        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
            enemy.sightTarget.transform.localPosition = Vector3.Lerp(startPosition, resetPosition, t);
            yield return null;
        }

        enemy.sightTarget.transform.localPosition = resetPosition;
    }
}