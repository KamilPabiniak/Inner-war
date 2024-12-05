using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    private IEnemyState currentState;
    public Transform target;
    protected bool isMachine;
    
    [Header("Patrol Settings")]
    public float patrolRange = 10f;
    public float waitTimeAtPatrolPoint = 2f;
    private Vector3 currentPatrolPoint;

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
        currentState?.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }
    
    public Vector3 RequestPatrolPoint()
    {
        return EnemyMediator.GetPatrolPoint(transform.position, patrolRange);
    }

    public void ReleasePatrolPoint(Vector3 point)
    {
        EnemyMediator.ReleasePatrolPoint(point);
    }
    
    public virtual void Patrol()
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
    
    private void OnDrawGizmos()
    {
        if (debugPatrolPoint && currentPatrolPoint != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(currentPatrolPoint, 2f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, currentPatrolPoint);
        }
    }
}