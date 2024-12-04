using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    private void Start()
    {
        isMachine = false;
        ChangeState(new PatrolState());
    }
}