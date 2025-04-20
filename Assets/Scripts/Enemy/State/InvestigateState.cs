using System.Collections;
using Anxiety;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.State
{
    public class InvestigateState : IEnemyState
    {
        private Vector3 _lastKnownPosition;
        private Coroutine _headRotationCoroutine;
        private float _lostSightTimer;

        private readonly float _initialRotationTime = 1.5f; 
        public InvestigateState(Vector3 position)
        {
            _lastKnownPosition  = position;
        }

        public void EnterState(EnemyBase enemy)
        {
            enemy.SetStateChangeLock(true);
            enemy.StartCoroutine(LookAtAlert(enemy));
            enemy.sound.PlayInvestigateSound();
            AnxietyManager.Instance.IncreaseFear(5f);
        }

        public void UpdateState(EnemyBase enemy)
        {
            if (enemy.Target != null && enemy.SeeTarget)
            {
                _lostSightTimer = 0f;
                _lastKnownPosition = enemy.Target.position;

                if (enemy.IsTargetInNavMesh(out NavMeshHit hit))
                {
                    if (!enemy.canMove || !(enemy.DetectionProgress > enemy.detectionValueToChase)) return;
                    enemy.FaceTarget();
                    enemy.navMeshAgent.SetDestination(hit.position);
                }
                else
                {
                    enemy.FaceTarget();
                }
            }
            else
            {
                _lostSightTimer += Time.deltaTime;
            
                if (_lostSightTimer < enemy.maxInvestigationTime)
                {
                    if (!enemy.IsTargetInNavMesh(out _)) return;
                    if (enemy.canMove && enemy.DetectionProgress > enemy.maxInvestigationTime)
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
        }
    
        private void RotateToAlert(EnemyBase enemy)
        {
            Vector3 direction = (_lastKnownPosition - enemy.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * enemy.rotationMultiplier);
        }
    }
}