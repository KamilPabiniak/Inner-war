using UnityEngine;
using Anxiety;

[RequireComponent(typeof(Transform))]
public class FearRangeDetector : MonoBehaviour
{
    [Header("Zakresy wykrywania")]
    public float wideRange = 10f;
    public float narrowRange = 5f;
    [Tooltip("Im mniejsze, tym bardziej p³asko (Y)")] 
    public float heightScale = 0.5f;

    [Space]
    public LayerMask enemyLayer;
    [SerializeField] private bool showSphere = true;

    private Collider[] _results = new Collider[16];
    private int _currentLevel; 

    private void Update()
    {
        if (_currentLevel == 3 || _currentLevel == 4)
            return;

        bool inNarrow = HasEnemyInRange(narrowRange);
        bool inWide   = !inNarrow && HasEnemyInRange(wideRange);
        
        int desired = inNarrow ? 2 
                    : inWide   ? 1 
                    : 0;
        
        if (desired != _currentLevel)
        {
            if (_currentLevel != 0)
                AnxietyManager.Instance.ClearActive(_currentLevel);

            if (desired != 0)
                AnxietyManager.Instance.TriggerActiveContinuous(desired);
            _currentLevel = desired;
        }
    }

    private bool HasEnemyInRange(float range)
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            range,
            _results,
            enemyLayer,
            QueryTriggerInteraction.Ignore);

        if (count == 0) return false;

        float rangeSqr = range * range;
        for (int i = 0; i < count; i++)
        {
            Vector3 dir = _results[i].transform.position - transform.position;
            dir.y *= heightScale; 
            if (dir.sqrMagnitude <= rangeSqr)
                return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        if (!showSphere) return;

        var oldMat = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(
            transform.position,
            Quaternion.identity,
            new Vector3(1f, heightScale, 1f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, wideRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, narrowRange);

        Gizmos.matrix = oldMat;
    }
}