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

    public InvestigateState(Vector3 position)
    {
        _lastKnownPosition  = position;
    }

    public void EnterState(EnemyBase enemy)
    {
        enemy.SetStateChangeLock(true);
    }

    public void UpdateState(EnemyBase enemy)
    {
        if (enemy.Target != null && enemy.seeTarget)
        {
            _lostSightTimer = 0f;
            _lastKnownPosition = enemy.Target.position;

            if (enemy.IsTargetInNavMesh(out NavMeshHit hit))
            {
                if (enemy.canMove && enemy.detectionProgress > enemy.detectionValueNeededToMoveToTarget)
                {
                    if (enemy.detectionProgress == enemy.detectionValueNeededToMoveToTarget)
                    {
                        enemy.sound.PlayInvestigateSound();
                    }
                    enemy.FaceTarget();
                    enemy.navMeshAgent.SetDestination(hit.position);
                }
            }
            else
            {
                enemy.FaceTarget();
                Debug.LogWarning($"[{enemy.name}] Gracz po za obszarem strze¿onym.");
            }
        }
        else
        {
            _lostSightTimer += Time.deltaTime;
            
            if (_lostSightTimer < enemy.maxInvestigationTimeAfterLoseSight)
            {
                if (!enemy.IsTargetInNavMesh(out NavMeshHit hit)) return;
                enemy.FaceTarget();
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
}