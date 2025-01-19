using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public abstract class EnemyBase : MonoBehaviour
{
    private IEnemyState currentState;
    public IEnemyState CurrentState => currentState;
    public event System.Action OnPlayerKilled;
    
    [Header("General Settings")]
    public NavMeshAgent navMeshAgent;
    public Transform Target { get; private set; }
    [Range(0f, 100f)]
    public float detectionProgress;
    public bool seeTarget;
    public bool canKill;
    public float killRadius = 1.4f;
    public bool canMove;
    public bool CanChangeState { get; private set; } = true;
    private bool _isChangingState;
    
    [Header("References")]
    public EnemySound sound;

    public CapsuleCollider collider;
    
    [Header("Patrol Settings")]
    public float patrolRange = 10f;
    [Tooltip("Minimalna odległość między punktami patrolowymi.")]
    [SerializeField] private float minPatrolPointDistance = 5f;
    public float waitTimeAtPatrolPoint = 2f;
    private Vector3 currentPatrolPoint;
    
    [FormerlySerializedAs("maxInvestigationTime")]
    [Header("Investigate Settings")]
    [Tooltip("Maksymalny czas w jakim zostanie w tym trybie po zgubieniu gracza. Musi być na wypadek buga")]
    public float maxInvestigationTimeAfterLoseSight = 10f;
    
    [Header("Attack Settings")]
    public float attackDuration = 5f;
    public float attackSpeedMultiplier = 1.5f;
  
    
    private void Awake()
    {
        EnemyPatrolM.RegisterEnemy(this);
        collider.radius = killRadius;
    }

    private void OnDestroy()
    {
        EnemyPatrolM.ReleasePatrolPoint(currentPatrolPoint);
        EnemyPatrolM.UnregisterEnemy(this);
    }

    private void Update()
    {
        currentState?.UpdateState(this);
    }
    
    public void SetStateChangeLock(bool locked)
    {
        CanChangeState = !locked;
    }

    public void ChangeState(IEnemyState newState)
    {
        if (_isChangingState || currentState == newState) return;
        _isChangingState = true;
        currentState?.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
        _isChangingState = false;
    }
    
    public Vector3 RequestPatrolPoint()
    {
        return EnemyPatrolM.GetPatrolPoint(transform.position, patrolRange, minPatrolPointDistance);
    }

    public bool CanSeeTarget()
    {
        return seeTarget;
    }
    
    public void SetTarget(Transform targetSet)
    {
        Target = targetSet;
    }

    public void ClearTarget()
    {
        Target = null;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!canKill) return;
        if (other.CompareTag("Player"))
        {
            AttemptKill(other);
        }
    }

    private void AttemptKill(Collider target)
    {
        PlayerDeath playerDeath = target.GetComponent<PlayerDeath>();
        if (playerDeath != null)
        {
            playerDeath.Kill();
            Debug.Log($"[{name}] Gracz został zabity.");
            OnPlayerKilled?.Invoke();
        }
    }

    public void OnAlertReceived(Vector3 alertPosition)
    {
        if (currentState is AttackState) return;
        ChangeState(new InvestigateState(alertPosition));
    }
    
    public void OnAttackCommandReceived(Transform player)
    {
        if (currentState is AttackState) return;
        SetTarget(player);
        ChangeState(new AttackState());
        detectionProgress = 100;
    }

    [ContextMenu("CurrentState")]
    public void TellCurrentState()
    {
        Debug.Log(currentState);
    }
}