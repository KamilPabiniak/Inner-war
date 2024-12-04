using UnityEngine;

public class EnemyFOV : MonoBehaviour
{
    [Header("FOV Settings")]
    [Range(0, 360)] public float viewAngle = 90f; // K¹t widzenia w stopniach.
    public LayerMask targetMask; // Warstwa celu (np. gracza).
    public LayerMask obstructionMask; // Warstwa przeszkód.

    [Header("Detection Settings")]
    public float viewRadius = 10f; // Zasiêg widzenia.
    public float detectionIncreaseRate = 10f; // Szybkoœæ wykrywania.
    public float detectionDecreaseRate = 5f; // Szybkoœæ redukcji wykrywania.
    public AnimationCurve distanceEffectMultiplier = AnimationCurve.Linear(0, 1, 10, 0.1f);

    [Header("Debug Options")]
    public bool debugFOV = true; // Czy rysowaæ FOV w Gizmos.
    public Color fovColor = Color.green; // Kolor obszaru FOV.
    public Color detectionColor = Color.red; // Kolor wykrytego celu.
    public bool debugConsole = true; // Czy logowaæ informacje.

    private float detectionProgress = 0f; // Progres wykrycia.
    private EnemyBase enemy;
    private bool isPlayerDetected;

    private void Awake()
    {
        enemy = GetComponent<EnemyBase>();
    }

    private void Update()
    {
        // Wykrywanie gracza, tylko gdy gracz jest w polu widzenia.
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        if (targetsInRange.Length > 0)
        {
            // Znalezienie najbli¿szego gracza
            Transform target = targetsInRange[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                // Sprawdzenie, czy nie ma przeszkód miêdzy wrogiem a graczem.
                if (!Physics.Raycast(transform.position, directionToTarget, viewRadius, obstructionMask))
                {
                    float distance = Vector3.Distance(transform.position, target.position);
                    float distanceMultiplier = distanceEffectMultiplier.Evaluate(distance);
                    detectionProgress += distanceMultiplier * detectionIncreaseRate * Time.deltaTime / Vector3.Distance(transform.position, target.position);
                    detectionProgress = Mathf.Clamp(detectionProgress, 0f, 100f);

                    // Debugowanie w konsoli
                    if (debugConsole)
                    {
                        Debug.Log($"[{name}] Wykrywanie gracza: {detectionProgress}%.");
                    }

                    // Jeœli wykrycie osi¹gnie 100%, zmieñ stan na atak
                    if (detectionProgress >= 100f)
                    {
                        enemy.ChangeState(new AttackState());
                    }
                }
            }
            else
            {
                // Jeœli gracz nie jest w k¹cie widzenia, zmniejszaj progres wykrycia.
                detectionProgress -= detectionDecreaseRate * Time.deltaTime;
            }
        }
        else
        {
            // Brak graczy w zasiêgu, zmniejszaj progres wykrycia.
            detectionProgress -= detectionDecreaseRate * Time.deltaTime;
        }

        detectionProgress = Mathf.Clamp(detectionProgress, 0f, 100f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugFOV) return;

        // Rysowanie okrêgu widzenia
        Gizmos.color = isPlayerDetected ? detectionColor : fovColor;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // Rysowanie granic k¹ta widzenia w oparciu o orientacjê wroga
        Vector3 leftBoundary = DirectionFromAngle(-viewAngle / 2, false); // U¿ywamy lokalnej przestrzeni
        Vector3 rightBoundary = DirectionFromAngle(viewAngle / 2, false); // U¿ywamy lokalnej przestrzeni
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);
    }

    private Vector3 DirectionFromAngle(float angleInDegrees, bool isGlobal)
    {
        if (!isGlobal)
        {
            // Jeœli nie jest globalny, uwzglêdniamy rotacjê obiektu
            angleInDegrees += transform.eulerAngles.y; // Obracamy o k¹t Y w lokalnej przestrzeni
        }
        float radianAngle = angleInDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radianAngle), 0, Mathf.Cos(radianAngle));
    }
}
