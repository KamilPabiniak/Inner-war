using UnityEngine;

public class RangedEnemy : EnemyBase
{
    private void Start()
    {
        ChangeState(new PatrolState());
    }
}