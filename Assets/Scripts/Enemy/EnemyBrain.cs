using UnityEngine;
using UnityEngine.AI;
using Enemy.State;

namespace Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(DetectionController))]
    public class EnemyBrain : MonoBehaviour
    {
        private readonly PatrolState _patrolState = new();
        private readonly InvestigateState _investigateState = new(Vector3.zero); 
        private readonly AttackState _attackState = new();
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
        public float investigateLockDuration = 10f;
        [Header("Attack")] 
        [Tooltip("Attack state duration before overload")]
        public float attackLockDuration = 5f;
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
        // internal lock state
        private IEnemyState _lockState;
        private float _lockExpiresAt;
        //play once Escaped
        private bool _escapeSoundPlayed;
        
        
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
            _currentState = _patrolState;
            _currentState.EnterState(this);
        }

        private void Update()
        {
            if (Target != null 
                && detection.IsPlayerVisible 
                && !IsTargetInNavMesh(out _))
            {
                movement.Stop();
                movement.Face(Target.position);
                
                if (!(_currentState is AttackState))
                {
                    audio.PlayWarningSound();
                }
                else
                {
                    if (_escapeSoundPlayed) return;
                    audio.PlayTargetEscapeSound();
                    _escapeSoundPlayed = true;
                }
                
                return;
            }
            
            if (_lockState != null && Time.time < _lockExpiresAt)
            {
                _currentState?.UpdateState(this);
                return;
            }
            if (_lockState != null && Time.time >= _lockExpiresAt)
            {
                _lockState = null;
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
                _investigateState.UpdatePosition(
                    detection.IsPlayerVisible && Target!=null
                        ? Target.position
                        : transform.position
                );
                RequestStateChange(_investigateState);
            }
            else // a <= 0f
            {
                RequestStateChange(_patrolState);
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

        private void UpdateVisuals(float progress, bool partial, bool full)
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
            
            detection.detectionMark.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
        }


        private void RequestStateChange(IEnemyState nextState)
        {
            if (_currentState == nextState || (_stateChangeRequested && _pendingState == nextState)) return;
            
            _pendingState = nextState;
            _stateChangeRequested = true;
        }
        
        private void PerformStateChange(IEnemyState nextState)
        {
            _escapeSoundPlayed = false;
            _currentState?.ExitState(this);
            _currentState = nextState;
            _stateChangeRequested = false;
            _pendingState = null;
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
            RequestStateChange(_patrolState);
        }
        
        public void OnAlertReceived(Vector3 alertPosition)
        {
            if (_currentState is AttackState) { return; }
            detection.SetAwarenessLevel(detectionValueToChase + 1f);
            PerformStateChange(_investigateState);
            _investigateState.UpdatePosition(alertPosition);
            _lockState = _investigateState;
            _lockExpiresAt = Time.time + investigateLockDuration;
        }

    
        public void OnAttackCommandReceived(Transform player)
        {
            if (_currentState is AttackState) return;
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
