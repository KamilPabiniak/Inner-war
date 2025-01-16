using UnityEditor;
using UnityEngine;

public static class SceneShelfCreator
{
    private const int MaxNameLength = 50;

    [MenuItem("GameObject/Create Shelf", false, 0)]
    private static void CreateShelfFromMenu()
    {
        string defaultName = "New Shelf";
        string formattedName = FormatShelfName(defaultName);
        GameObject shelfObject = new GameObject(formattedName);
        shelfObject.transform.hideFlags = HideFlags.HideInInspector;
        Selection.activeGameObject = shelfObject;
        MonitorNameChange(shelfObject);
    }

    private static string FormatShelfName(string name)
    {
        int dashCount = Mathf.Clamp((MaxNameLength - name.Length) / 2, 3, MaxNameLength / 2);
        string dashes = new string('-', dashCount);
        return $"{dashes}{name}{dashes}";
    }

    private static void MonitorNameChange(GameObject obj)
    {
        EditorApplication.hierarchyChanged += () =>
        {
            if (obj != null && !obj.name.StartsWith("-") && !obj.name.EndsWith("-"))
            {
                obj.name = FormatShelfName(obj.name);
            }
        };
    }

    [MenuItem("GameObject/Create Shelf", true)]
    private static bool ValidateCreateShelfFromMenu()
    {
        return !Application.isPlaying;
    }
}