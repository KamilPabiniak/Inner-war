using UnityEngine;

public class MachineEnemy : EnemyBase
{
    private void Start()
    {
        isMachine = true;
        ChangeState(new PatrolState());
    }

    public override void ChaseTarget()
    {
        base.ChaseTarget();
        if (target != null)
        {
            EnemyMediator.SendAlert(target.position);
        }
    }
}