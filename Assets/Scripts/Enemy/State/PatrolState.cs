using UnityEngine;

public class PatrolState : IEnemyState
{
    public void EnterState(EnemyBase enemy)
    {
        Debug.Log($"[{enemy.name}] Wchodzi w stan patrolowania.");
        //enemy.SetLightColor(Color.white);
    }

    public void UpdateState(EnemyBase enemy)
    {
        enemy.Patrol();
    }

    public void ExitState(EnemyBase enemy)
    {
        Debug.Log($"[{enemy.name}] Opuszcza stan patrolowania.");
    }
}