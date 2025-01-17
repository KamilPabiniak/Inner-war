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
        int totalLength = MaxNameLength;
        int dashCount = Mathf.Max(0, totalLength - name.Length);
        int leftDashCount = dashCount / 2;
        int rightDashCount = dashCount - leftDashCount;

        string leftDashes = new string('-', leftDashCount);
        string rightDashes = new string('-', rightDashCount);
        return leftDashes + name + rightDashes;
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