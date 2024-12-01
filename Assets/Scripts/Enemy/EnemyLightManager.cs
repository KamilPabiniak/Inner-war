using System.Collections.Generic;
using UnityEngine;

public class EnemyLightManager
{
    private static List<Light> enemyLights = new List<Light>();

    public static void RegisterEnemyLight(Light light)
    {
        if (light != null && !enemyLights.Contains(light))
        {
            enemyLights.Add(light);
        }
    }

    public static void UnregisterEnemyLight(Light light)
    {
        if (light != null && enemyLights.Contains(light))
        {
            enemyLights.Remove(light);
        }
    }

    public static List<Light> GetEnemyLights()
    {
        return new List<Light>(enemyLights);
    }
}
