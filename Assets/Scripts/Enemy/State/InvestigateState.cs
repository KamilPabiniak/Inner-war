using UnityEngine;

public class InvestigateState : IEnemyState
{
    private Vector3 alertPosition;

    public InvestigateState(Vector3 position)
    {
        alertPosition = position;
    }

    public void EnterState(EnemyBase enemy)
    {
        Debug.Log($"[{enemy.name}] Idê sprawdziæ miejsce alarmu.");
    }

    public void UpdateState(EnemyBase enemy)
    {
        enemy.Patrol(); 
    }

    public void ExitState(EnemyBase enemy)
    {
        Debug.Log($"[{enemy.name}] Opuszcza stan sprawdzania alarmu.");
    }
}