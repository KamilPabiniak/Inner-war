using UnityEngine;
using UnityEngine.AI;

namespace Enemy.Type
{
    public class MachineEnemy : EnemyBase
    {
        [AdvancedHeader("Machine Specification", fontSize: 16, bottomSpace: 15f, alignment: TextAnchor.MiddleLeft, foldable: true)]
        [Header("References")]
        public GameObject sightTarget;
        public Vector3 OriginalHeadPos { get; private set; }

        [AdvancedHeader("Patrol Head Specification", fontSize: 11, bottomSpace: 15f, alignment: TextAnchor.MiddleLeft, foldable: false , hideMe: true)]
        public float headRotationSpeed;
        public int stopPoints;
        public float stopDuration = 0.5f;
        [Range(1,5)]
        [Tooltip("Maksymalna odleg³oœc punktu SightTarget")]
        public int maxOffsetDistance = 3; 
        [AdvancedHeader("Attack", fontSize: 11, bottomSpace: 15f, alignment: TextAnchor.MiddleLeft)]
        public float overloadTimer = 6f;
        
        private void Start()
        {
            OriginalHeadPos = sightTarget.transform.localPosition;
            ChangeState(new PatrolState());
        }

        private void FixedUpdate()
        {
            UpdateDetectionState();
        }

        private void UpdateDetectionState()
        {
            detectionProgress = Mathf.Clamp(detectionProgress, 0f, 100f);

            if (detectionProgress <= 0.1f && CurrentState is not PatrolState)
            {
                if (!CanChangeState) return;
                ChangeState(new PatrolState());
            }
            else if (detectionProgress > 0.1f && detectionProgress < 100f && CurrentState is not InvestigateState)
            {
                if (!CanChangeState) return;
                ChangeState(new InvestigateState(Player != null ? Player.position : transform.position));
            }
            else if (detectionProgress >= 100f && CurrentState is not AttackState)
            {
                if (!IsTargetInNavMesh(out NavMeshHit hit)) return;
                ChangeState(new AttackState());
            }
        }

        [ContextMenu("Patrol")]
        public void ForcePatrol()
        {
            ChangeState(new PatrolState());
        }
    }
}

