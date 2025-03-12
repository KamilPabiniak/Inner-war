using Enemy.State;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.Type
{
    public class MachineEnemy : EnemyBase
    {
        [Binder("Machine Specification", fontSize: 16, bottomSpace: 5f, topSpace: 40f, alignment: TextAnchor.MiddleLeft, fontStyle: FontStyle.Bold, foldAll: true, colorHex:"#a1adff")]
        
        [Binder("Reference", bottomSpace: 1f, alignment: TextAnchor.MiddleLeft , fontStyle: FontStyle.BoldAndItalic, colorHex: "#ffeca1")]
        public GameObject sightTarget;
        public Vector3 OriginalHeadPos { get; private set; }
        
        [Header("Patrol Head Specification")]
        public float headRotationSpeed;
        public int stopPoints;
        public float stopDuration = 0.5f;
        [Range(1,5)]
        [Tooltip("Max distance from SightTarget")]
        public int maxOffsetDistance = 3; 
      
        
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

