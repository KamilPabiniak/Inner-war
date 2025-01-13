using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public abstract class EnemyBase : MonoBehaviour
{
    private IEnemyState currentState;
    public IEnemyState CurrentState => currentState;
    
    [Header("General Settings")]
    public NavMeshAgent navMeshAgent;
    public Transform target { get; private set; }
    public bool seeTarget;
    public bool canKill;
    public bool canMove;
    public bool CanChangeState { get; private set; } = true;
    private bool _isChangingState;
    
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

    [AdvancedHeader("Debug Settings", r:180f, g:1f, b: 180f)]
    [SerializeField] private bool enableConsoleDebug;
    [SerializeField] private bool patrolDebug;
    [SerializeField] private bool debugPatrolPoint;
    [SerializeField] private bool investigateDebug;
    [SerializeField] private bool chaseDebug;
    
    [Header("References")]
    public EnemySoundManager soundManager;
    
    private void Awake()
    {
        EnemyPatrolMediator.RegisterEnemy(this);
    }

    private void OnDestroy()
    {
        EnemyPatrolMediator.ReleasePatrolPoint(currentPatrolPoint);
        EnemyPatrolMediator.UnregisterEnemy(this);
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
        return EnemyPatrolMediator.GetPatrolPoint(transform.position, patrolRange, minPatrolPointDistance);
    }

    public bool CanSeeTarget()
    {
        return seeTarget;
    }
    
    public void SetTarget(Transform targetSet)
    {
        target = targetSet;
    }

    public void ClearTarget()
    {
        target = null;
    }

    public void OnAlertReceived(Vector3 alertPosition)
    {
        if (enableConsoleDebug)
        {
            Debug.Log($"[{name}] Otrzymano alarm! Ruszam do: {alertPosition}.");
        }
        //ChangeState(new InvestigateState(alertPosition));
    }

    [ContextMenu("CurrentState")]
    public void TellCurrentState()
    {
        Debug.Log(currentState);
    }
}