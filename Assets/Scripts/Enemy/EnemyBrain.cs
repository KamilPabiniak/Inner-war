using UnityEngine;
using UnityEngine.AI;
using Enemy.State;

namespace Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(DetectionController))]
    public class EnemyBrain : MonoBehaviour
    {
        public Transform Target { get; private set; }
        
        [Header("References")] 
        public EnemyAudio audio;
        public DetectionController detection;
        public MovementController  movement;
        public Animator animator;     
        private IEnemyState CurrentState { get; set; }

        [Header("Common Settings")] 
        public bool canKill = true;
        public bool canMove = true;
        private bool _wasPlayerVisible;
        private bool _canChangeState = true, _isChangingState;
        
        // Patrol & Investigate & Attack Settings
        [Header("Patrol")] 
        public float patrolRange = 10f;
        public float minPatrolPointDistance = 2f;
        public float waitTimeAtPatrolPoint = 3f;
        [Header("Investigate")] 
        [Tooltip("Detection progress at which enemy switches to investigate")]
        public float detectionValueToChase = 25f;
        [Tooltip("Time before giving up investigation")]
        public float maxInvestigationTime = 10f;
        [Header("Attack")] 
        [Tooltip("Attack state duration before overload")]
        public float attackDuration = 5f;
        public float attackAfterLostTarget = 5f;
        [Tooltip("NavMeshAgent speed multiplier during attack")]
        public float attackSpeedMultiplier = 1.5f;
        [Tooltip("Time to wait after overloaded attack")]
        public float waitAfterAttack = 6f;

        private Vector3 _currentPatrolPoint;
        
        private void Awake() => EnemyPatrolHandler.RegisterEnemy(this);

        private void OnEnable()
        {
            detection.OnPartial    += HandlePartial;
            detection.OnLostPartial+= HandleLostPartial;
            detection.OnFull       += HandleFull;
            detection.OnProgress   += HandleProgress;
            detection.OnSpotted    += SetTarget;
        }

        private void OnDisable()
        {
            EnemyPatrolHandler.ReleasePatrolPoint(_currentPatrolPoint);
            EnemyPatrolHandler.UnregisterEnemy(this);
            detection.OnPartial    -= HandlePartial;
            detection.OnLostPartial-= HandleLostPartial;
            detection.OnFull       -= HandleFull;
            detection.OnProgress   -= HandleProgress;
            detection.OnSpotted    -= SetTarget;
        }

        private void Start()
        {
            ChangeState(new PatrolState());
        }

        void Update()
        {
            TryStateTransition();
            CurrentState?.UpdateState(this);
        }
        
        private void SetTarget(Transform t) => Target = t;
        
        // -- State Management --
        private void TryStateTransition()
        {
            if (!_canChangeState) return;

            float awareness = detection.AwarenessLevel;

            // 1) If we’re fully alert → Attack
            if (awareness >= 100f && !(CurrentState is AttackState))
            {
                ChangeState(new AttackState());
            }
            else if (awareness >= detectionValueToChase && !(CurrentState is InvestigateState))
            {
                Vector3 alertPos = (detection.IsPlayerVisible && Target != null)
                    ? Target.position
                    : transform.position;
                ChangeState(new InvestigateState(alertPos));
            }
            else if (!(CurrentState is PatrolState))
            {
                ChangeState(new PatrolState());
            }
        }

        
        private void HandlePartial() => UpdateVisuals(detection.AwarenessLevel, partial: true, full: false);

        private void HandleLostPartial() => UpdateVisuals(detection.AwarenessLevel, partial: false, full: false);

        private void HandleFull() => UpdateVisuals(detection.AwarenessLevel, partial: false, full: true);

        private void HandleProgress(float progress)
        {
            UpdateVisuals(progress, 
                partial: progress >= detection.threshold, 
                full:    progress >= 100f);
        }

        private void UpdateVisuals(float prog, bool partial, bool full) {
            if (full) {
                detection.lightComponent.color = Color.red;
                detection.detectionMark.color = Color.red;
            }
            else if (partial) {
                detection.lightComponent.color = Color.yellow;
                detection.detectionMark.color = Color.Lerp(Color.white, Color.yellow, prog/100f);
            }
            else {
                detection.lightComponent.color = Color.white;
                var c =  detection.detectionMark.color; c.a = 0;  detection.detectionMark.color = c;
            }
        }

        public void ChangeState(IEnemyState next)
        {
            if (_isChangingState || CurrentState == next) return;
            _isChangingState = true;
            CurrentState?.ExitState(this);
            CurrentState = next;
            CurrentState.EnterState(this);
            _isChangingState = false;
        }

        // -- Helpers --
        public bool IsTargetInNavMesh(out NavMeshHit hit)
        {
            if (Target != null 
                && NavMesh.SamplePosition(Target.position, out hit, 1f, NavMesh.AllAreas) 
                && Vector3.Distance(Target.position, hit.position) < 1f)
            {
                return true;
            }
            hit = new NavMeshHit();
            return false;
        }
        
        public void SetStateChangeLock(bool locked) => _canChangeState = !locked;
        public Vector3 RequestPatrolPoint() => EnemyPatrolHandler.GetPatrolPoint(transform.position, patrolRange, minPatrolPointDistance);
        
        private void OnTriggerEnter(Collider other)
        {
            if (!canKill) return;
            if (other.CompareTag("Player"))
            {
                var death = other.GetComponent<PlayerDeath>();
                if (death != null)
                {
                    death.Kill();
                    GameEvents.onPlayerKilled?.Invoke();
                    SetTarget(null);
                }
            }
        }
        
        //Commands
        public void OnAlertReceived(Vector3 alertPosition)
        {
            if (CurrentState is AttackState) return;
            ChangeState(new InvestigateState(alertPosition));
        }
    
        public void OnAttackCommandReceived(Transform player)
        {
            if (CurrentState is AttackState) return;
            SetTarget(player);
            ChangeState(new AttackState()); 
            detection.SetAwarenessLevel(100f);
        }
        
        [ContextMenu("CurrentState")]
        public void TellCurrentState() => Debug.Log(CurrentState);
        
        [ContextMenu("Investigate")]
        public void ForceInvestigate()
        {
            Transform target = Target.gameObject.transform;
            SetTarget(target);
            OnAlertReceived(target.position);
        }
    
        [ContextMenu("Attack")]
        public void ForceAttack()
        {
            Transform target = Target.gameObject.transform;
            OnAttackCommandReceived(target);
        }
    }
}
