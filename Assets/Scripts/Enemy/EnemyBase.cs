using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBase : MonoBehaviour
{
    private IEnemyState currentState;
    public IEnemyState CurrentState => currentState;
    
    public  Transform target { get; private set; }
    public bool seeTarget;
    private bool isChangingState;
    
    [Header("Patrol Settings")]
    public float patrolRange = 10f;
    [Tooltip("Minimalna odległość między punktami patrolowymi.")]
    [SerializeField] private float minPatrolPointDistance = 5f;
    public float waitTimeAtPatrolPoint = 2f;
    public NavMeshAgent navMeshAgent;
    private Vector3 currentPatrolPoint;
    
    [Header("Investigate Settings")]
    [Tooltip("Czas oczekiwania w ostatniej znanej pozycji gracza.")]
    public float waitTimeAtInvestigation = 3f;
    
    [Header("Attack Settings")]
    public float attackDuration = 5f;
    public float attackSpeedMultiplier = 1.5f;


    [Header("Debug Settings")]
    [SerializeField] private bool enableConsoleDebug;
    [SerializeField] private bool patrolDebug;
    [SerializeField] private bool debugPatrolPoint;
    [SerializeField] private bool investigateDebug;
    [SerializeField] private bool chaseDebug;
    
    private void Awake()
    {
        EnemyMediator.RegisterEnemy(this);
    }

    private void OnDestroy()
    {
        EnemyMediator.ReleasePatrolPoint(currentPatrolPoint);
        EnemyMediator.UnregisterEnemy(this);
    }

    private void Update()
    {
        currentState?.UpdateState(this);
    }

    public void ChangeState(IEnemyState newState)
    {
        if (isChangingState || currentState == newState) return;
        isChangingState = true;
        currentState?.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
        isChangingState = false;
    }
    
    public Vector3 RequestPatrolPoint()
    {
        return EnemyMediator.GetPatrolPoint(transform.position, patrolRange, minPatrolPointDistance);
    }

    public bool CanSeeTarget()
    {
        if (seeTarget)
        {
            return true;
        }
        else
        {
            return false;
        }
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
}