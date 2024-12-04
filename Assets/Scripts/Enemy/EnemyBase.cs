using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    private IEnemyState currentState;
    public Transform target;
    protected Light enemyLight;
    protected bool isMachine;

    [Header("Debug Settings")]
    [SerializeField] private bool enableConsoleDebug;
    [SerializeField] private bool patrolDebug;
    [SerializeField] private bool investigateDebug;
    [SerializeField] private bool chaseDebug;

    private void Awake()
    {
        enemyLight = GetComponent<Light>();
        EnemyMediator.RegisterEnemy(this);
    }

    private void OnDestroy()
    {
        EnemyMediator.UnregisterEnemy(this);
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

    public void SetLightColor(Color color)
    {
        if (isMachine && enemyLight != null)
        {
            enemyLight.color = color;
        }
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
}