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
        public NavMeshAgent navMeshAgent;
        public Light lightComponent;
        [SerializeField] private SpriteRenderer detectionMark;
        public GameObject sightTarget;
        
        private readonly Collider[] _senseBuffer = new Collider[4];
        private Color _markColor;
        public Vector3 OriginalHeadPos { get; private set; }
        private IEnemyState CurrentState { get; set; }
        public Transform Target { get; private set; }

        [Header("Common Settings")] 
        public bool canKill = true;
        public bool canMove = true;
        public float rotationMultiplier = 5f;
        public float DetectionProgress { get; private set; }
        public bool SeeTarget { get; private set; }
        private bool _canChangeState = true, _isChangingState;

        [Header("Light detection")] 
        public LayerMask targetMask;
        [Range(0f, 1f)] public float detectionThreshold = 0.1f;
        public float detectionIncreaseRate = 10f;
        public float detectionDecreaseRate = 5f;
        public AnimationCurve distanceMultiplier = AnimationCurve.Linear(0,1,10,0.1f);

        [Header("Head Scan")] 
        public float headRotationSpeed = 1f;
        public int stopPoints = 3;
        public float stopDuration = 0.5f;
        public int maxOffsetDistance = 3;
        public float headDetectionDistance = 5f;

        [Header("Wall Avoiding")] 
        public float mainDetectionDistance = 6f;
        public float sideDetectionDistance = 4f;
        public float rayOriginHeight = 1.5f;
        public float sideRayAngleOffset = 30f;
        public float additionalRayAngleOffset = 20f;

        [Header("Debug")] 
        public bool debugFOV = true, debugWallRays = true;
        public Color fovColor = Color.green;

        // Patrol & Investigate & Attack Settings
        [Header("Patrol")] 
        public static readonly float PatrolRange = 10f;
        public static readonly float MinPatrolPointDistance = 2f;
        public static readonly float WaitTimeAtPatrolPoint = 3f;
        [Header("Investigate")] 
        [Tooltip("Detection progress at which enemy switches to investigate")]
        public float detectionValueToChase = 25f;
        [Tooltip("Time before giving up investigation")]
        public float maxInvestigationTime = 10f;
        [Header("Attack")] 
        [Tooltip("Attack state duration before overload")]
        public float attackDuration = 5f;
        [Tooltip("NavMeshAgent speed multiplier during attack")]
        public float attackSpeedMultiplier = 1.5f;
        [Tooltip("Time to wait after overloaded attack")]
        public float waitAfterAttack = 6f;

        private Vector3 _currentPatrolPoint;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            _markColor = detectionMark.color;
            EnemyPatrolHandler.RegisterEnemy(this);
        }

        private void Start()
        {
            OriginalHeadPos = sightTarget.transform.localPosition;
            ChangeState(new PatrolState());
        }

        private void Update()
        {
            CurrentState?.UpdateState(this);
            SenseTarget();
            UpdateDetectionProgress();
            TryStateTransition();
            UpdateVisuals();
        }

        // -- Sensing & Detection --
        private void SenseTarget()
        {
            Transform detected = null;
            Vector3 origin = lightComponent.transform.position;
            float range = lightComponent.range;
            
            int count = Physics.OverlapSphereNonAlloc(origin, range, _senseBuffer, targetMask);
            for (int i = 0; i < count; i++)
            {
                var col = _senseBuffer[i];
                if (!col.CompareTag("Player")) continue;
                Vector3 dir = (col.transform.position - origin).normalized;
                if (lightComponent.type == LightType.Spot &&
                    Vector3.Angle(lightComponent.transform.forward, dir) > lightComponent.spotAngle * 0.5f)
                    continue;
                if (Physics.Raycast(origin, dir, out RaycastHit hit, range) && hit.transform == col.transform)
                {
                    detected = col.transform;
                    break;
                }
            }
            Target = detected;
        }

        void UpdateDetectionProgress()
        {
            float I = Target ? CalculateIntensity(Target) : 0f;
            // Debug intensity
            // Debug.Log($"Intensity: {I:F2}, Threshold: {detectionThreshold:F2}");

            if (I > detectionThreshold)
            {
                SeeTarget = true;
                DetectionProgress += I * detectionIncreaseRate * Time.deltaTime;
            }
            else
            {
                SeeTarget = false;
                DetectionProgress -= detectionDecreaseRate * Time.deltaTime;
            }
            DetectionProgress = Mathf.Clamp(DetectionProgress, 0f, 100f);
        }

        float CalculateIntensity(Transform t)
        {
            Vector3 origin = lightComponent.transform.position;
            Vector3 dir = (t.position - origin).normalized;
            if (lightComponent.type == LightType.Spot &&
                Vector3.Angle(lightComponent.transform.forward, dir) > lightComponent.spotAngle * 0.5f)
                return 0f;
            if (Physics.Raycast(origin, dir, out RaycastHit hit, lightComponent.range) && hit.transform == t)
                return (lightComponent.intensity / (hit.distance * hit.distance))
                       * distanceMultiplier.Evaluate(hit.distance);
            return 0f;
        }


        // -- State Management --
        void TryStateTransition()
        {
            if (!_canChangeState) return;
            // Full bar → Attack
            if (DetectionProgress >= 100f &&
                !(CurrentState is AttackState) &&
                IsTargetInNavMesh(out _))
            {
                ChangeState(new AttackState());
            }
            // Above chase threshold → Investigate
            else if (DetectionProgress >= detectionValueToChase &&
                     DetectionProgress < 100f &&
                     !(CurrentState is InvestigateState))
            {
                ChangeState(new InvestigateState(
                    Target ? Target.position : transform.position));
            }
            // Below chase threshold → Patrol
            else if (DetectionProgress < detectionValueToChase &&
                     !(CurrentState is PatrolState))
            {
                ChangeState(new PatrolState());
            }
        }
        
        void UpdateVisuals()
        {
            float t = DetectionProgress / 100f;
            lightComponent.color = CurrentState switch
            {
                PatrolState _ => Color.white,
                InvestigateState _ => Color.yellow,
                AttackState _ => Color.red,
                _ => lightComponent.color
            };
            _markColor = CurrentState switch
            {
                PatrolState _ => new Color(_markColor.r, _markColor.g, _markColor.b, 0f),
                InvestigateState _ => Color.Lerp(Color.white, Color.yellow, t),
                AttackState _ => Color.red,
                _ => _markColor
            };
            if (!(CurrentState is PatrolState)) _markColor.a = 1f;
            detectionMark.color = _markColor;
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

        private void SetTarget(Transform t) => Target = t;
        public void SetStateChangeLock(bool locked) => _canChangeState = !locked;
        public Vector3 RequestPatrolPoint() => EnemyPatrolHandler.GetPatrolPoint(transform.position, PatrolRange, MinPatrolPointDistance);


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
            DetectionProgress = 100;
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
            
            Vector3 originLight = lightComponent.transform.position;
            // Draw detection range sphere
            Gizmos.color = fovColor;
            Gizmos.DrawWireSphere(originLight, lightComponent.range);

            // Draw spot light cone
            if (lightComponent.type == LightType.Spot)
            {
                float halfAngle = lightComponent.spotAngle * 0.5f;
                Vector3 forwardLight = lightComponent.transform.forward;
                DrawConeRay(originLight, forwardLight, halfAngle);
                DrawConeRay(originLight, Quaternion.Euler(0, halfAngle, 0) * forwardLight, halfAngle);
                DrawConeRay(originLight, Quaternion.Euler(0, -halfAngle, 0) * forwardLight, halfAngle);
                DrawConeRay(originLight, Quaternion.Euler(halfAngle, 0, 0) * forwardLight, halfAngle);
                DrawConeRay(originLight, Quaternion.Euler(-halfAngle, 0, 0) * forwardLight, halfAngle);
            }


            if (!debugWallRays) return;
            Vector3 origin = transform.position + Vector3.up * rayOriginHeight;
            var forward = transform.forward;
            // Main forward ray
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(origin, forward * mainDetectionDistance);

            // Side rays
            Gizmos.color = Color.cyan;
            Vector3 leftDir  = Quaternion.Euler(0, -sideRayAngleOffset, 0) * forward;
            Vector3 rightDir = Quaternion.Euler(0,  sideRayAngleOffset, 0) * forward;
            Gizmos.DrawRay(origin, leftDir * sideDetectionDistance);
            Gizmos.DrawRay(origin, rightDir * sideDetectionDistance);

            // Additional machine-specific rays
            Gizmos.color = Color.magenta;
            Vector3 extraL = Quaternion.Euler(0, -additionalRayAngleOffset, 0) * forward;
            Vector3 extraR = Quaternion.Euler(0,  additionalRayAngleOffset, 0) * forward;
            Gizmos.DrawRay(origin, extraL * mainDetectionDistance);
            Gizmos.DrawRay(origin, extraR * mainDetectionDistance);
        }
        
        private void DrawConeRay(Vector3 origin, Vector3 direction, float unused)
        {
            Gizmos.color = fovColor;
            Gizmos.DrawRay(origin, direction.normalized * lightComponent.range);
        }
    }
}
