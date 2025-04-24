using UnityEngine;
using UnityEngine.AI;
using Enemy.State;

namespace Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(DetectionController))]
    public class EnemyBrain : MonoBehaviour
    {
        private static readonly PatrolState PatrolState = new();
        private static readonly InvestigateState InvestigateState = new(Vector3.zero); 
        private static readonly AttackState AttackState = new();
        public Transform Target { get; private set; }
        private IEnemyState _currentState;
        
        [Header("References")] 
        public new EnemyAudio audio;
        public DetectionController detection;
        public MovementController  movement;
        public Animator animator;     

        [Header("Common Settings")] 
        public bool canKill = true;
        public bool canMove = true;
        
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
        public float waitAfterOverload = 6f;
        
        private Vector3 _currentPatrolPoint;
        private bool _wasPlayerVisible;
        private bool _stateChangeRequested;
        private IEnemyState _pendingState;
        
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
            _currentState = PatrolState;
            _currentState.EnterState(this);
        }

        void Update()
        {
            TryStateTransition();
            _currentState?.UpdateState(this);
            if (_stateChangeRequested)
            {
                PerformStateChange(_pendingState);
                _stateChangeRequested = false;
            }
        }
        
        private void SetTarget(Transform t) => Target = t;
        
        // -- State Management --
        private void TryStateTransition()
        {
            if (_currentState is AttackState)
                return;

            float awareness = detection.AwarenessLevel;

            if (awareness >= 100f)
            {
                if (!(_currentState is AttackState))
                {
                    RequestStateChange(AttackState);
                }
                return;
            }

            if (awareness >= detectionValueToChase)
            {
                Vector3 alertPos = detection.IsPlayerVisible && Target != null
                    ? Target.position
                    : transform.position;
                InvestigateState.UpdatePosition(alertPos);
                RequestStateChange(InvestigateState);
                return;
            }
            
            if (!(_currentState is PatrolState))
            {
                RequestStateChange(PatrolState);
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

        private void RequestStateChange(IEnemyState nextState)
        {
            if (_currentState == nextState) return;
            
            _pendingState = nextState;
            _stateChangeRequested = true;
        }

        private void PerformStateChange(IEnemyState nextState)
        {
            _currentState?.ExitState(this);
            _currentState = nextState;
            _currentState.EnterState(this);
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
        public void OnBackToPatrol()
        {
            RequestStateChange(PatrolState);
        }
        
        public void OnAlertReceived(Vector3 alertPosition)
        {
            if (_currentState is AttackState) return;
            RequestStateChange(InvestigateState);
            InvestigateState.UpdatePosition(alertPosition);
        }
    
        public void OnAttackCommandReceived(Transform player)
        {
            if (_currentState is AttackState) return;
            SetTarget(player);
            RequestStateChange(AttackState); 
            detection.SetAwarenessLevel(100f);
        }
        
        [ContextMenu("CurrentState")]
        public void TellCurrentState() => Debug.Log(_currentState);
        
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
