using UnityEngine;
using UnityEngine.AI;
using Enemy.State;

namespace Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(DetectionController))]
    public class EnemyBrain : MonoBehaviour
    {
        private readonly PatrolState _patrolState = new();
        private readonly InvestigateState _investigateState = new(); 
        private readonly AttackState _attackState = new();
        private IEnemyState _currentState;
        public Transform Target { get; private set; }

        [Header("Info")] 
        [SerializeField] private string currentStateInfo;
        
        [Header("References")] 
        public new EnemyAudio audio;
        public DetectionController detection;
        public MovementController  movement;
        public Animator animator;     
        
        [Header("Optional Patrol Area")]
        public PatrolArea patrolAreaOverride;

        [Header("Common Settings")] 
        public bool canKill = true;
        public bool canMove = true;
        public float increaseFearValueOnSpotted;
        [field: Header("Animation Multiplayer")] 
        public float animationWalkSpeedPatrol;
        public float animationWalkSpeedInvestigate;
        public float animationWalkSpeedAttack;
        
        // Patrol & Investigate & Attack Settings
        [Space, Header("Patrol")] 
        [Tooltip("Radius in which the opponent can select a new patrol point")]
        public float patrolRange = 10f;
        [Tooltip("Minimum distance the new patrol point must be from the current position of the enemy or other opponents")]
        public float minPatrolPointDistance = 2f;
        [Tooltip("The length of time an opponent �waits� in place after reaching a patrol point before moving on to the next one")]
        public float waitTimeAtPatrolPoint = 3f;
        public float waitBeforeMove = 3f;
        [Header("Investigate")] 
        [Tooltip("Detection progress at which enemy switches to investigate")]
        public float detectionValueToChase = 25f;
        [Tooltip("Time before giving up investigation")]
        public float investigateLockDuration = 10f;
        [Header("Attack")] 
        [Tooltip("Attack state duration before overload")]
        public float attackLockDuration = 5f;
        [Tooltip("The length of time an opponent continues the pursuit even if they have lost the player from sight")]
        public float attackAfterLostTarget = 5f;
        [Tooltip("NavMeshAgent speed multiplier during attack")]
        public float attackSpeedMultiplier = 1.5f;
        [Tooltip("Time to wait after overloaded attack")]
        public float waitAfterOverload = 6f;
        
        //Patrol
        private Vector3 _currentPatrolPoint;
        //Visibility
        private bool _wasPlayerVisible;
        //State Change
        private bool _stateChangeRequested;
        private IEnemyState _pendingState;
        //Internal lock state
        private IEnemyState _lockState;
        private float _lockExpiresAt;
        
        
        private void Awake()
        {
            EnemyPatrolHandler.RegisterEnemy(this);
        }

        private void OnEnable()
        {
            detection.OnProgress   += HandleProgress;
            detection.OnSpotted    += SetTarget;
        }

        private void OnDisable()
        {
            EnemyPatrolHandler.ReleasePatrolPoint(_currentPatrolPoint);
            EnemyPatrolHandler.UnregisterEnemy(this);
            detection.OnProgress   -= HandleProgress;
            detection.OnSpotted    -= SetTarget;
        }

        private void Start()
        {
            _currentState = _patrolState;
            _currentState.EnterState(this);
        }

        private void Update()
        {
            currentStateInfo = _currentState.ToString();
            if (Target != null 
                && detection.IsPlayerVisible 
                && !IsTargetInNavMesh(out _))
            {
                if (_currentState is AttackState { IsOverloading: true }) return;
                movement.Stop();
                movement.Face(Target.position);
                audio.PlayWarningSound(); 
                return;
            }
            
            // Lock on Investigate/Attack
            if (_lockState != null)
            {
                // --- NEW: break attack lock if target is outside NavMesh ---
                if (_lockState is AttackState && Target != null && !IsTargetInNavMesh(out _))
                {
                    _lockState = null;
                }
                else if (Time.time < _lockExpiresAt)
                {
                    _currentState.UpdateState(this);
                    return;
                }
                else
                {
                    _lockState = null;
                }
            }


            TryStateTransition();
            _currentState?.UpdateState(this);
            if (_stateChangeRequested)
            {
                PerformStateChange(_pendingState);
            }
        }

        
        private void SetTarget(Transform t) => Target = t;
        
        // -- State Management --
        private void TryStateTransition()
        {
            if (_currentState is AttackState) return;

            float awarenessLevel = detection.AwarenessLevel;

            if (awarenessLevel >= 100f && IsTargetInNavMesh(out _))
            {
                RequestStateChange(_attackState);
            }
            else if (awarenessLevel > 0f)   //  (0,100)
            {
                if (detection.IsPlayerVisible && Target != null)
                {
                    _investigateState.UpdatePosition(Target.position);
                }
                RequestStateChange(_investigateState);
            }
            else // a <= 0f
            {
                RequestStateChange(_patrolState);
            }
        }

        private void HandleProgress(float progress) => UpdateVisuals(progress);

        private void UpdateVisuals(float progress)
        {
            Color targetColor;
            float alpha = Mathf.Clamp01(progress / 100f);

            if (_currentState is AttackState)
            {
                targetColor = Color.red;
                alpha = 1f; 
            }
            else if (_currentState is InvestigateState)
            {
                targetColor = Color.yellow;
                alpha = 1f; 
            }
            else
            {
                targetColor = Color.white;
            }
            
            detection.lightComponent.color = targetColor;
        }


        private void RequestStateChange(IEnemyState nextState)
        {
            if (_currentState == nextState || (_stateChangeRequested && _pendingState == nextState)) return;
            
            _pendingState = nextState;
            _stateChangeRequested = true;
        }
        
        private void PerformStateChange(IEnemyState nextState)
        {
            if (_currentState == nextState) return;
            bool wasHighAlert = _currentState is AttackState or InvestigateState;
            bool willHighAlert = nextState is AttackState or InvestigateState;
            
            _currentState?.ExitState(this);
            
            if (!wasHighAlert && willHighAlert)
                GameEvents.onHighAlertStart?.Invoke();

            if (wasHighAlert && !willHighAlert)
                GameEvents.onHighAlertEnd?.Invoke();
            
            _currentState = nextState;
            _stateChangeRequested = false;
            _pendingState = null;
            _currentState.EnterState(this);
            if (_lockState != _currentState) { _lockState = null; }
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
                    detection.SetAwarenessLevel(0f);
                }
            }
        }
        
        //Commands
        public void OnBackToPatrol()
        {
            SetTarget(null);
            RequestStateChange(_patrolState);
        }
        
        public void OnAlertReceived(Vector3 alertPosition)
        {
            if (_currentState is AttackState or InvestigateState) { return; }
            detection.SetAwarenessLevel(detectionValueToChase + 20f);
            PerformStateChange(_investigateState);
            _investigateState.UpdatePosition(alertPosition);
        }
    
        public void OnAttackCommandReceived(Transform player)
        {
            if (_currentState is AttackState && !IsTargetInNavMesh(out _)) return;
            _lockState = null;
            SetTarget(player);
            detection.SetAwarenessLevel(100f);
            PerformStateChange(_attackState);
            _lockState = _attackState;
            _lockExpiresAt = Time.time + attackLockDuration;
        }
        
        [ContextMenu("CurrentState")]
        public void TellCurrentState() => Debug.Log(_currentState);
        
        [ContextMenu("Investigate")]
        public void ForceInvestigate()
        {
            SetTarget(Player.Instance.transform);
            Transform target = Target.gameObject.transform;
            SetTarget(target);
            OnAlertReceived(target.position);
        }
    
        [ContextMenu("Attack")]
        public void ForceAttack()
        {
            SetTarget(Player.Instance.transform);
            Transform target = Target.gameObject.transform;
            OnAttackCommandReceived(target);
        }
        
        //Gizmo
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, patrolRange);
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, minPatrolPointDistance);
        }

    }
}
