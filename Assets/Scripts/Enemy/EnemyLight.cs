using UnityEngine;

[RequireComponent(typeof(Light))]
public class EnemyLight : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Prêdkoœæ wzrostu wykrycia.")]
    public float detectionIncreaseRate = 10f;
    [Tooltip("Prêdkoœæ spadku wykrycia.")]
    public float detectionDecreaseRate = 5f;
    [Tooltip("Minimalny próg wykrycia œwiat³a.")]
    public float detectionThreshold = 0.1f;
    [Tooltip("Mno¿nik efektu wykrywania w zale¿noœci od odleg³oœci.")]
    public AnimationCurve distanceEffectMultiplier = AnimationCurve.Linear(0, 1, 10, 0.1f);

    [Header("Debug Settings")]
    [Tooltip("Czy wyœwietlaæ debugowe linie Raycastów?")]
    public bool debugRays = true;
    [Tooltip("Kolor linii Raycastów trafiaj¹cych w gracza.")]
    public Color rayHitColor = Color.green;
    [Tooltip("Kolor linii Raycastów, które nie trafiaj¹ w gracza.")]
    public Color rayMissColor = Color.red;

    private Light lightComponent;
    private float detectionProgress = 0f;

    private void Awake()
    {
        lightComponent = GetComponent<Light>();
        EnemyLightManager.RegisterEnemyLight(lightComponent);
    }

    private void OnDestroy()
    {
        EnemyLightManager.UnregisterEnemyLight(lightComponent);
    }

    public void UpdateDetectionState(Transform target)
    {
        float lightIntensity = CalculateLightIntensity(target);

        if (lightIntensity > detectionThreshold)
        {
            detectionProgress += lightIntensity * detectionIncreaseRate * Time.deltaTime;
        }
        else
        {
            detectionProgress -= detectionDecreaseRate * Time.deltaTime;
        }

        detectionProgress = Mathf.Clamp(detectionProgress, 0f, 100f);

        if (detectionProgress >= 50f && detectionProgress < 100f)
        {
            Debug.Log($"[{name}] Wykrywanie na poziomie 50%!");
        }
        else if (detectionProgress >= 100f)
        {
            Debug.Log($"[{name}] Wykrywanie na poziomie 100%!");
        }

        UpdateLightAppearance();
    }

    private float CalculateLightIntensity(Transform target)
    {
        Vector3 directionToTarget = (target.position - lightComponent.transform.position).normalized;

        if (lightComponent.type == LightType.Spot)
        {
            float angle = Vector3.Angle(lightComponent.transform.forward, directionToTarget);
            if (angle > lightComponent.spotAngle / 2f)
            {
                DrawDebugRay(lightComponent.transform.position, directionToTarget, false);
                return 0f;
            }
        }

        RaycastHit hit;
        if (Physics.Raycast(lightComponent.transform.position, directionToTarget, out hit, lightComponent.range))
        {
            if (hit.transform != target)
            {
                DrawDebugRay(lightComponent.transform.position, directionToTarget, false);
                return 0f;
            }

            DrawDebugRay(lightComponent.transform.position, directionToTarget, true);
        }
        else
        {
            DrawDebugRay(lightComponent.transform.position, directionToTarget, false);
        }

        float distance = Vector3.Distance(lightComponent.transform.position, target.position);
        float distanceMultiplier = distanceEffectMultiplier.Evaluate(distance);
        float intensity = lightComponent.intensity / Mathf.Pow(distance, 2f);

        return intensity * distanceMultiplier;
    }

    private void UpdateLightAppearance()
    {
        Color newColor = Color.white;

        if (detectionProgress >= 50f && detectionProgress < 100f)
        {
            newColor = Color.yellow;
        }
        else if (detectionProgress >= 100f)
        {
            newColor = Color.red;
        }

        lightComponent.color = newColor;
    }

    private void DrawDebugRay(Vector3 origin, Vector3 direction, bool hit)
    {
        if (!debugRays) return;

        Color color = hit ? rayHitColor : rayMissColor;
        Debug.DrawLine(origin, origin + direction * 10f, color, 0.1f);
    }
}
