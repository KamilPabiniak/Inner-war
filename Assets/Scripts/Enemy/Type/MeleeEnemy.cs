using Enemy;
using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    private void Start()
    {
        ChangeState(new PatrolState());
    }
}