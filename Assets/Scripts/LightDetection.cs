using UnityEngine;

public class EnemyLightDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Minimalny próg wykrycia światła.")]
    public float detectionThreshold = 0.1f;
    [Tooltip("Prędkość wzrostu/zmniejszania wykrycia.")]
    public float detectionRate = 10f;

    [Header("Debug Settings")]
    [Tooltip("Czy wyświetlać debugowe linie Raycastów?")]
    public bool debugRays = true;
    [Tooltip("Kolor linii Raycastów trafiających w obiekt.")]
    public Color rayHitColor = Color.green;
    [Tooltip("Kolor linii Raycastów, które nie trafiają w obiekt.")]
    public Color rayMissColor = Color.red;

    private float detectionProgress = 0f;

    private void Update()
    {
        float totalLightIntensity = CalculateEnemyLightIntensity();
        UpdateDetectionProgress(totalLightIntensity);
    }

    private float CalculateEnemyLightIntensity()
    {
        float intensitySum = 0f;
        var enemyLights = EnemyLightManager.GetEnemyLights();

        foreach (var light in enemyLights)
        {
            intensitySum += CalculateLightContribution(light);
        }

        return Mathf.Clamp01(intensitySum);
    }

    private float CalculateLightContribution(Light light)
    {
        Vector3 directionToDetector = (transform.position - light.transform.position).normalized;

        if (light.type == LightType.Spot)
        {
            float angle = Vector3.Angle(light.transform.forward, directionToDetector);
            if (angle > light.spotAngle / 2f)
            {
                DrawDebugRay(light.transform.position, directionToDetector, false);
                return 0f; // Poza kątem reflektora
            }
        }

        RaycastHit hit;
        if (Physics.Raycast(light.transform.position, directionToDetector, out hit, light.range))
        {
            if (hit.transform != transform)
            {
                DrawDebugRay(light.transform.position, directionToDetector, false);
                return 0f; // Gracz nie został trafiony
            }

            DrawDebugRay(light.transform.position, directionToDetector, true);
        }
        else
        {
            DrawDebugRay(light.transform.position, directionToDetector, false);
        }

        // Oblicz intensywność na podstawie odległości
        float distance = Vector3.Distance(light.transform.position, transform.position);
        return light.intensity / Mathf.Pow(distance, 2f);
    }

    private void UpdateDetectionProgress(float intensity)
    {
        if (intensity > detectionThreshold)
        {
            detectionProgress += intensity * detectionRate * Time.deltaTime;
        }
        else
        {
            detectionProgress -= detectionRate * Time.deltaTime;
        }

        detectionProgress = Mathf.Clamp(detectionProgress, 0f, 100f);

        if (detectionProgress >= 50f && detectionProgress < 100f)
        {
            Debug.Log("Wykrywanie na poziomie 50%!");
        }
        else if (detectionProgress >= 100f)
        {
            Debug.Log("Wykrywanie na poziomie 100%!");
        }

        UpdateLightColor();
    }

    private void UpdateLightColor()
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

        foreach (var light in EnemyLightManager.GetEnemyLights())
        {
            light.color = newColor;
        }
    }

    private void DrawDebugRay(Vector3 origin, Vector3 direction, bool hit)
    {
        if (!debugRays) return;

        Color color = hit ? rayHitColor : rayMissColor;
        Debug.DrawLine(origin, origin + direction * 10f, color, 0.1f); // Rysuje linię na scenie
    }

    private void OnDrawGizmos()
    {
        if (!debugRays) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f); // Reprezentacja gracza w debugowaniu

        // Rysuje linie do każdego wroga (tylko do debugowania na scenie, nie w czasie rzeczywistym)
        foreach (var light in EnemyLightManager.GetEnemyLights())
        {
            if (light != null)
            {
                Gizmos.color = rayHitColor;
                Gizmos.DrawLine(light.transform.position, transform.position);
            }
        }
    }
}
