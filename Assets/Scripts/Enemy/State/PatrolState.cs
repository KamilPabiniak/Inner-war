using UnityEngine;

public class PatrolState : IEnemyState
{
    private Vector3 patrolPoint;
    private bool isWaiting;
    private float waitTimer;
    public void EnterState(EnemyBase enemy)
    {
        Debug.Log($"[{enemy.name}] Wchodzi w stan patrolowania.");
        SetNewPatrolPoint(enemy);
    }

    public void UpdateState(EnemyBase enemy)
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                SetNewPatrolPoint(enemy);
            }
            return;
        }

        if (Vector3.Distance(enemy.transform.position, patrolPoint) < 1f)
        {
            isWaiting = true;
            waitTimer = enemy.waitTimeAtPatrolPoint;
            enemy.ReleasePatrolPoint(patrolPoint);
        }
        else
        {
            Vector3 direction = (patrolPoint - enemy.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 5f);
            
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, patrolPoint, 3f * Time.deltaTime);
        }
        //enemy.Patrol();
    }

    public void ExitState(EnemyBase enemy)
    {
        Debug.Log($"[{enemy.name}] Opuszcza stan patrolowania.");
        enemy.ReleasePatrolPoint(patrolPoint);
    }
    
    private void SetNewPatrolPoint(EnemyBase enemy)
    {
        patrolPoint = enemy.RequestPatrolPoint();
        Debug.Log($"[{enemy.name}] Nowy punkt patrolowy: {patrolPoint}.");
    }
}