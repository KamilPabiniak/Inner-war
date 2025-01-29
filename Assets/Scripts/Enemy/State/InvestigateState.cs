using System.Collections;
using Enemy;
using Enemy.Type;
using UnityEngine;
using UnityEngine.AI;

public class InvestigateState : IEnemyState
{
    private Vector3 _lastKnownPosition;
    private Coroutine _headRotationCoroutine;
    private float _lostSightTimer;

    private float _initialRotationTime = 1.5f; 
    private bool _finishedLookingAtAlert;
    public InvestigateState(Vector3 position)
    {
        _lastKnownPosition  = position;
    }

    public void EnterState(EnemyBase enemy)
    {
        enemy.SetStateChangeLock(true);
        enemy.StartCoroutine(LookAtAlert(enemy));
    }

    public void UpdateState(EnemyBase enemy)
    {
        if (enemy.Player != null && enemy.seeTarget)
        {
            _lostSightTimer = 0f;
            _lastKnownPosition = enemy.Player.position;

            if (enemy.IsTargetInNavMesh(out NavMeshHit hit))
            {
                if (enemy.canMove && enemy.detectionProgress > enemy.detectionValueNeededToMoveToTarget)
                {
                    if (enemy.detectionProgress == enemy.detectionValueNeededToMoveToTarget)
                    {
                        enemy.sound.PlayInvestigateSound();
                    }
                    enemy.FacePlayer();
                    enemy.navMeshAgent.SetDestination(hit.position);
                }
            }
            else
            {
                enemy.FacePlayer();
                Debug.LogWarning($"[{enemy.name}] Gracz po za obszarem strze¿onym.");
            }
        }
        else
        {
            _lostSightTimer += Time.deltaTime;
            
            if (_lostSightTimer < enemy.maxInvestigationTimeAfterLoseSight)
            {
                if (!enemy.IsTargetInNavMesh(out NavMeshHit hit)) return;
                if (enemy.canMove && enemy.detectionProgress > enemy.detectionValueNeededToMoveToTarget)
                {
                    enemy.navMeshAgent.SetDestination(_lastKnownPosition);
                }
            }
            else
            {
                enemy.ChangeState(new PatrolState());
            }
        }
    }

    public void ExitState(EnemyBase enemy)
    {
        enemy.SetStateChangeLock(false);
    }
    
    private IEnumerator LookAtAlert(EnemyBase enemy)
    {
        float timer = 0f;
        while (timer < _initialRotationTime)
        {
            RotateToAlert(enemy);
            timer += Time.deltaTime;
            yield return null;
        }
        _finishedLookingAtAlert = true;
    }
    
    private void RotateToAlert(EnemyBase enemy)
    {
        Vector3 direction = (_lastKnownPosition - enemy.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * enemy.rotationMultiplier);
    }
}