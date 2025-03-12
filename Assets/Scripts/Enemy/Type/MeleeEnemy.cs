using Enemy;
using Enemy.State;
using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    private void Start()
    {
        ChangeState(new PatrolState());
    }
}