using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public abstract class EnemyBase : MonoBehaviour
    {
        public IEnemyState CurrentState { get; private set; }
    
        [Header("General Settings")]
        public NavMeshAgent navMeshAgent;
        public Transform Player { get; private set; }
        [Range(0f, 100f)] // Detection here
        public float detectionProgress;
        public bool seeTarget;
        public bool canKill;
        public float killRadius = 1.4f;
        public bool canMove;
        [Tooltip("Gdy przeciwnik ma obserwować lub obracać się szybko do celu. Np. Investigate ma moment gdy gracz jest po za obszarem to do niego sie odwróć")]
        public float rotationMultiplier = 5f;
        protected bool CanChangeState { get; private set; } = true;
        private bool _isChangingState;
    
        [Header("References")]
        public EnemySound sound;
        [SerializeField] private CapsuleCollider killTrigger;
        [SerializeField] private SpriteRenderer detectionMark;
        private Color _markColor;
    
        [Header("Patrol Settings")]
        public float patrolRange = 10f;
        [Tooltip("Minimalna odległość między punktami patrolowymi.")]
        [SerializeField] private float minPatrolPointDistance = 5f;
        public float waitTimeAtPatrolPoint = 2f;
        private Vector3 _currentPatrolPoint;
    
        [Header("Investigate Settings")]
        [Tooltip("Wartość wykrycia jaka musi być przy wykryciu by przeciwnik zaczął iść do celu")]
        public float detectionValueNeededToMoveToTarget = 25f;
        [Tooltip("Maksymalny czas w jakim zostanie w tym trybie po zgubieniu gracza. Musi być na wypadek buga")]
        public float maxInvestigationTimeAfterLoseSight = 10f;
    
        [Header("Attack Settings")]
        public float attackDuration = 5f;
        public float attackSpeedMultiplier = 1.5f;
    
        private void Awake()
        {
            EnemyPatrolHandler.RegisterEnemy(this);
            killTrigger.radius = killRadius;
            _markColor = detectionMark.color;
        }

        private void OnDestroy()
        {
            EnemyPatrolHandler.ReleasePatrolPoint(_currentPatrolPoint);
            EnemyPatrolHandler.UnregisterEnemy(this);
        }

        private void Update()
        {
            CurrentState?.UpdateState(this);
            
            // Smoothly interpolate between white (at progress 0) and yellow (at progress 1)
            float progress = Mathf.Clamp01(detectionProgress / 100f);
            
            switch (CurrentState)
            {
                case PatrolState:
                    _markColor.a = 0f;
                    detectionMark.color = _markColor;
                    break;
                case InvestigateState:
                    _markColor.a = 255f;
                    _markColor = Color.Lerp(Color.white, Color.yellow, progress);
                    detectionMark.color = _markColor;
                    break;
                case AttackState:
                    _markColor.a = 255f;
                    _markColor = Color.red;
                    detectionMark.color = _markColor;
                    break;
                default:
                    Debug.LogWarning($"Unhandled state: {CurrentState.GetType().Name}");
                    break;
            }
        }
    
        public void SetStateChangeLock(bool locked) => CanChangeState = !locked;

        public void ChangeState(IEnemyState newState)
        {
            if (_isChangingState || CurrentState == newState) return;
            _isChangingState = true;
            CurrentState?.ExitState(this);
            CurrentState = newState;
            CurrentState.EnterState(this);
            _isChangingState = false;
        }
    
        public Vector3 RequestPatrolPoint() => EnemyPatrolHandler.GetPatrolPoint(transform.position, patrolRange, minPatrolPointDistance);

        public void SetTarget(Transform target)
        {
            Player = target;

            seeTarget = target != null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!canKill) return;
            if (other.CompareTag("Player"))
            {
                AttemptKill(other);
            }
        }

        private void AttemptKill(Component target)
        {
            var playerDeath = target.GetComponent<PlayerDeath>();
            if (playerDeath == null) return;
            playerDeath.Kill();
            GameEvents.onPlayerKilled?.Invoke();
            SetTarget(null);
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
            detectionProgress = 100;
        }
    
        public bool IsTargetInNavMesh(out NavMeshHit hit)
        {
            if (Player != null)
            {
                bool isOnNavMesh = NavMesh.SamplePosition(Player.position, out hit, 1f, NavMesh.AllAreas);
                // Jeśli SamplePosition znalazło punkt, ale odległość jest większa niż próg, zwróć false
                if (isOnNavMesh && Vector3.Distance(Player.position, hit.position) < 1f)
                {
                    return true;
                }
            }

            hit = default;
            return false;
        }
    
        public void FacePlayer()
        {
            if (Player == null) return;

            // Określamy kierunek do gracza, ignorując oś Y
            Vector3 direction = (Player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationMultiplier);
        }
    
        /*//For IsTargetInNavMesh
        private void OnDrawGizmos()
        {
            // if (Target != null)
            // {
            //     Gizmos.color = Color.red;
            //     Gizmos.DrawSphere(Target.position, 1f);
            //
            //     if (IsTargetInNavMesh(out NavMeshHit hit))
            //     {
            //         Gizmos.color = Color.green;
            //         Gizmos.DrawSphere(hit.position, 1f);
            //     }
            // }
        }*/
    
        [ContextMenu("CurrentState")]
        public void TellCurrentState() => Debug.Log(CurrentState);
        
        [ContextMenu("Investigate")]
        public void ForceInvestigate()
        {
            Transform target = FindAnyObjectByType(typeof(Player)).GameObject().gameObject.transform;
            SetTarget(target);
            OnAlertReceived(target.position);
        }
    
        [ContextMenu("Attack")]
        public void ForceAttack()
        {
            Transform target = FindAnyObjectByType(typeof(Player)).GameObject().gameObject.transform;
            OnAttackCommandReceived(target);
        }
    }
}