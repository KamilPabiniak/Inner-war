using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBase : MonoBehaviour
{
    private IEnemyState currentState;
    public Transform target;
    protected bool isMachine;
    private bool isChangingState = false;
    
    [Header("Patrol Settings")]
    public float patrolRange = 10f;
    [Tooltip("Minimalna odleg³oœæ miêdzy punktami patrolowymi.")]
    [SerializeField] private float minPatrolPointDistance = 5f;
    public float waitTimeAtPatrolPoint = 2f;
    public NavMeshAgent navMeshAgent;
    private Vector3 currentPatrolPoint;
    
    [Header("Investigate Settings")]
    [Tooltip("Czas oczekiwania w ostatniej znanej pozycji gracza.")]
    public float waitTimeAtInvestigation = 3f;

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
        EnemyMediator.UnregisterEnemy(this);
        EnemyMediator.ReleasePatrolPoint(currentPatrolPoint);
    }

    private void Update()
    {
        currentState?.UpdateState(this);
    }

    public void ChangeState(IEnemyState newState)
    {
        if (isChangingState) return;
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
    
    public void Patrol()
    {
        if (enableConsoleDebug && patrolDebug)
        {
            Debug.Log($"[{name}] Patroluje.");
        }
    }

    public virtual void ChaseTarget()
    {
        if (target != null)
        {
            if (enableConsoleDebug && chaseDebug)
            {
                Debug.Log($"[{name}] Œciga cel: {target.name}.");
            }
        }
    }

    public void OnAlertReceived(Vector3 alertPosition)
    {
        if (enableConsoleDebug)
        {
            Debug.Log($"[{name}] Otrzymano alarm! Ruszam do: {alertPosition}.");
        }
        ChangeState(new InvestigateState(alertPosition));
    }
}