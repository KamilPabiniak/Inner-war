using UnityEngine;

[RequireComponent(typeof(Light))]
public class EnemyLight : MonoBehaviour
{
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
}