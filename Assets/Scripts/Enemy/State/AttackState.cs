using UnityEngine;

public class AttackState : IEnemyState
{
    public void EnterState(EnemyBase enemy)
    {
        Debug.Log($"[{enemy.name}] Wchodzi w stan ataku!");
        //enemy.SetLightColor(Color.red);
    }

    public void UpdateState(EnemyBase enemy)
    {
        enemy.ChaseTarget();
    }

    public void ExitState(EnemyBase enemy)
    {
        Debug.Log($"[{enemy.name}] Opuszcza stan ataku.");
    }
}