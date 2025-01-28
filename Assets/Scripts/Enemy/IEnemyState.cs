using Enemy;

public interface IEnemyState
{
    void EnterState(EnemyBase enemy);
    void UpdateState(EnemyBase enemy);
    void ExitState(EnemyBase enemy);
}