using UnityEngine;
using UnityEngine.AI;
using Enemy.State;

namespace Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBase : MonoBehaviour
    {
        [Header("References")]
        public EnemySound sound;
        [SerializeField] private SpriteRenderer detectionMark;
        private Color _markColor;
        public Vector3 OriginalHeadPos { get; private set; }
        protected internal NavMeshAgent navMeshAgent;
        public IEnemyState CurrentState { get; private set; }
        public Transform Target { get; private set; }

        [Header("Common Settings")]
        public bool canKill = true;
        public bool canMove = true;
        public float rotationMultiplier = 5f;
        public float DetectionProgress { get; protected set; }
        public bool SeeTarget { get; protected set; }
        protected bool CanChangeState { get; private set; } = true;
        private bool _isChangingState;
        
        [Header("Light detection")]
        [Range(0f, 100f)]
        public float detectionProgress;
        public GameObject sightTarget;
        public Light lightComponent;
        public LayerMask targetMask;
        [Tooltip("Detection per second")] public float detectionIncreaseRate = 10f;
        [Tooltip("Decay per second")]     public float detectionDecreaseRate = 5f;
        [Tooltip("Min light intensity")]  public float detectionThreshold = 0.1f;
        public AnimationCurve distanceMultiplier = AnimationCurve.Linear(0,1,10,0.1f);

        [Header("Head Scan")]
        public float headRotationSpeed = 1f;
        public int stopPoints = 3;
        public float stopDuration = 0.5f;
        public int maxOffsetDistance = 3;
        public float headDetectionDistance = 5f;

        [Header("Wall avoiding")]
        public float mainDetectionDistance = 6f;
        public float sideDetectionDistance = 4f;
        public float rayOriginHeight = 1.5f;
        public float sideRayAngleOffset = 30f;
        public float additionalRaycastAngleOffset = 20f;

        [Header("Debug")]
        public bool debugFOV = true, debugWallRays = true, debugRays = true;
        public Color fovColor = Color.green;
        public Color rayHitColor = Color.green, rayMissColor = Color.red;

        // Patrol settings (instance-based)
        [Header("Patrol Settings")]
        public static readonly float PatrolRange = 10f;
        [Tooltip("Minimum distance between patrol points")]
        public static readonly float MinPatrolPointDistance = 2f;
        [Tooltip("Wait time at patrol point")]
        public static readonly float WaitTimeAtPatrolPoint = 3f;

        [Header("Investigate Settings")]
        [Tooltip("Detection value to start moving to target")]
        public float detectionValueNeededToMoveToTarget = 25f;
        [Tooltip("Max investigate time after losing sight")]
        public float maxInvestigationTimeAfterLoseSight = 10f;
    
        [Header("Attack Settings")]
        public float attackDuration = 5f;
        public float attackSpeedMultiplier = 1.5f;
        public float waitingAfterAttack = 6f;
        private Vector3 _currentPatrolPoint;

        protected virtual void Awake()
        {
            EnemyPatrolHandler.RegisterEnemy(this);
            _markColor = detectionMark.color;
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
        
        private void Start()
        {
            ChangeState(new PatrolState());
            OriginalHeadPos = sightTarget.transform.localPosition;
        }

        private void Update()
        {
            CurrentState?.UpdateState(this);
            
            float progress = Mathf.Clamp01(detectionProgress / 100f);
            // 1) Sense
            var hits = Physics.OverlapSphere(transform.position, lightComponent.range, targetMask);
            SetTarget(hits.Length > 0 ? hits[0].transform : null);

            // 2) Update detectionProgress
            if (Target) UpdateDetection(); 
            else        DetectionProgress -= detectionDecreaseRate * Time.deltaTime;

            DetectionProgress = Mathf.Clamp(DetectionProgress, 0f, 100f);

            // 3) Visual feedback
            if (CurrentState is PatrolState)
                lightComponent.color = Color.white;
            else if (CurrentState is InvestigateState)
                lightComponent.color = Color.yellow;
            else if (CurrentState is AttackState)
                lightComponent.color = Color.red;

            // 4) State?switch logic
            if (!CanChangeState) return;

            if (DetectionProgress <= detectionThreshold * 100f && !(CurrentState is PatrolState))
                ChangeState(new PatrolState());
            else if (DetectionProgress > detectionThreshold * 100f
                     && DetectionProgress < 100f
                     && !(CurrentState is InvestigateState))
                ChangeState(new InvestigateState(Player.Instance.transform.position));
            else if (DetectionProgress >= 100f
                     && !(CurrentState is AttackState)
                     && IsTargetInNavMesh(out _))
                ChangeState(new AttackState());
            
            if (CurrentState is PatrolState)
            {
                _markColor.a = 0f;
            }
            else if (CurrentState is InvestigateState)
            {
                _markColor.a = 1f;
                _markColor = Color.Lerp(Color.white, Color.yellow, progress);
            }
            else if (CurrentState is AttackState)
            {
                _markColor.a = 1f;
                _markColor = Color.red;
            }
            detectionMark.color = _markColor;
        }
        
        private void UpdateDetection()
        {
            float I = CalculateLightIntensity(Target);
            if (I > detectionThreshold)
            {
                DetectionProgress += I * detectionIncreaseRate * Time.deltaTime;
                SeeTarget = true;
            }
            else
            {
                DetectionProgress -= detectionDecreaseRate * Time.deltaTime;
                SeeTarget = false;
            }
        }

        private float CalculateLightIntensity(Transform t)
        {
            Vector3 dir = (t.position - lightComponent.transform.position).normalized;
            if (lightComponent.type == LightType.Spot)
            {
                float a = Vector3.Angle(lightComponent.transform.forward, dir);
                if (a > lightComponent.spotAngle * .5f)
                {
                    DebugRay(lightComponent.transform.position, dir, false);
                    return 0f;
                }
            }
            // occlusion
            if (Physics.Raycast(lightComponent.transform.position, dir, out var hit, lightComponent.range))
            {
                bool ok = hit.transform == t;
                DebugRay(lightComponent.transform.position, dir, ok);
                if (!ok) return 0f;
            }
            else
            {
                DebugRay(lightComponent.transform.position, dir, false);
                return 0f;
            }
            // attenuation
            float d = Vector3.Distance(lightComponent.transform.position, t.position);
            return (lightComponent.intensity / (d * d)) * distanceMultiplier.Evaluate(d);
        }

        private void DebugRay(Vector3 o, Vector3 d, bool hit)
        {
            if (!debugRays) return;
            Debug.DrawLine(o, o + d * lightComponent.range, hit ? rayHitColor : rayMissColor, .1f);
        }

        public void ChangeState(IEnemyState newState)
        {
            if (_isChangingState || CurrentState == newState) return;
            _isChangingState = true;
            CurrentState?.ExitState(this);
            CurrentState = newState;
            CurrentState.EnterState(this);
            _isChangingState = false;
        }
        
        public Vector3 RequestPatrolPoint()
        {
            _currentPatrolPoint = EnemyPatrolHandler.GetPatrolPoint(this);
            return _currentPatrolPoint;
        }

        public void SetTarget(Transform t)
        {
            Target = t;
            SeeTarget = t != null;
        }

        public bool IsTargetInNavMesh(out NavMeshHit hit)
        {
            if (Target != null &&
                NavMesh.SamplePosition(Target.position, out hit, 1f, NavMesh.AllAreas) &&
                Vector3.Distance(Target.position, hit.position) < 1f)
            {
                return true;
            }
            hit = default;
            return false;
        }

        public void FaceTarget()
        {
            if (Target == null) return;
            Vector3 dir = (Target.position - transform.position).normalized;
            Quaternion look = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * rotationMultiplier);
        }

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

        public void SetStateChangeLock(bool locked) => CanChangeState = !locked;
        
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
            detectionProgress = 100;
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
        
        private void OnDestroy()
        {
            EnemyPatrolHandler.ReleasePatrolPoint(_currentPatrolPoint);
            EnemyPatrolHandler.UnregisterEnemy(this);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!debugFOV) return;
            // Draw FOV sphere
            Gizmos.color = fovColor;
            Gizmos.DrawWireSphere(transform.position, lightComponent.range);

            if (!debugWallRays) return;
            // Draw wall detection rays
            Vector3 origin = transform.position + Vector3.up * rayOriginHeight;
            
            // Main forward ray
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(origin, transform.forward * mainDetectionDistance);

            // Side rays
            Gizmos.color = Color.cyan;
            Vector3 leftDir  = Quaternion.Euler(0, -sideRayAngleOffset, 0) * transform.forward;
            Vector3 rightDir = Quaternion.Euler(0,  sideRayAngleOffset, 0) * transform.forward;
            Gizmos.DrawRay(origin, leftDir * sideDetectionDistance);
            Gizmos.DrawRay(origin, rightDir * sideDetectionDistance);

            // Additional machine-specific rays
            Gizmos.color = Color.magenta;
            Vector3 extraL = Quaternion.Euler(0, -additionalRaycastAngleOffset, 0) * transform.forward;
            Vector3 extraR = Quaternion.Euler(0,  additionalRaycastAngleOffset, 0) * transform.forward;
            Gizmos.DrawRay(origin, extraL * mainDetectionDistance);
            Gizmos.DrawRay(origin, extraR * mainDetectionDistance);
        }
    }
}
