using UnityEngine;

public class EnemyTarget : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Obiekt, który ma być wykrywany (zazwyczaj gracz).")]
    public Transform target;

    private void Update()
    {
        if (!target) return;

        var enemyLights = EnemyLightManager.GetEnemyLights();

        foreach (var detector in enemyLights)
        {
            var enemyLightComponent = detector.GetComponent<EnemyLight>();
            if (enemyLightComponent != null)
            {
                enemyLightComponent.UpdateDetectionState(target);
            }
        }
    }
}
