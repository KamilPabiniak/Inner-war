using UnityEngine;

public class RangedEnemy : EnemyBase
{
    private void Start()
    {
        isMachine = false;
        ChangeState(new PatrolState());
    }
}