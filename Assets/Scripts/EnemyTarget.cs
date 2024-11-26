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

        foreach (var light in enemyLights)
        {
            var enemyLightComponent = light.GetComponent<EnemyLight>();
            if (enemyLightComponent != null)
            {
                enemyLightComponent.UpdateDetectionState(target);
            }
        }
    }
}
