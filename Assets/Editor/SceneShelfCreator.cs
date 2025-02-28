using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tool for creating and updating shelf names based on the width of the Hierarchy window.
/// Each shelf has a ShelfIdentifier component that stores its unique identifier and base name.
/// This ensures that the data persists between scene changes or editor restarts.
/// </summary>
[InitializeOnLoad]
public static class SceneShelfCreator
{
    // Configurable settings
    private const float Margin = 50f;
    private const string ShelfSymbol = "-"; // Symbol used for formatting the names
    private const string DefaultShelfName = "New Shelf";
    private const double DebounceThreshold = 0.1; // Debounce delay threshold (in seconds) for updates
    private static double _lastUpdateCallTime;
    private static float _lastHierarchyWidth = -1f;
    private static bool _eventsSubscribed;

    static SceneShelfCreator()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.delayCall += OnEditorInitialized;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        _lastHierarchyWidth = GetHierarchyWindowWidth();
        EnsureEventsSubscribed();
        UpdateAllShelves();
    }

    private static void OnEditorInitialized()
    {
        EnsureEventsSubscribed();
        UpdateAllShelves();
    }

    [MenuItem("GameObject/Create Shelf", false, 0)]
    private static void CreateShelfFromMenu()
    {
        // Format the name based on the base name
        string formattedName = FormatShelfName(DefaultShelfName);
        GameObject shelfObject = new GameObject(formattedName)
        {
            transform = { hideFlags = HideFlags.HideInInspector }
        };

        // Add the ShelfIdentifier component
        ShelfIdentifier identifier = shelfObject.AddComponent<ShelfIdentifier>();
        identifier.baseName = DefaultShelfName;
        if (string.IsNullOrEmpty(identifier.uniqueID))
            identifier.uniqueID = System.Guid.NewGuid().ToString();

        // Register the operation with the Undo system
        Undo.RegisterCreatedObjectUndo(shelfObject, "Create Shelf");
        Selection.activeGameObject = shelfObject;
        _lastHierarchyWidth = GetHierarchyWindowWidth();

        EnsureEventsSubscribed();
    }

    [MenuItem("GameObject/Create Shelf", true)]
    private static bool ValidateCreateShelfFromMenu()
    {
        return !Application.isPlaying;
    }

    /// <summary>
    /// Subscribes to events if they have not been added yet and if at least one shelf exists.
    /// </summary>
    private static void EnsureEventsSubscribed()
    {
        if (!_eventsSubscribed && Object.FindObjectsByType<ShelfIdentifier>(FindObjectsSortMode.None).Length > 0)
        {
            EditorApplication.update += UpdateShelvesOnResize;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            _eventsSubscribed = true;
        }
    }

    /// <summary>
    /// Updates shelf names when the Hierarchy window size changes.
    /// The debounce mechanism prevents excessive calls.
    /// </summary>
    private static void UpdateShelvesOnResize()
    {
        double currentTime = EditorApplication.timeSinceStartup;
        if (currentTime - _lastUpdateCallTime < DebounceThreshold)
            return;
        _lastUpdateCallTime = currentTime;

        float currentWidth = GetHierarchyWindowWidth();
        if (!Mathf.Approximately(currentWidth, _lastHierarchyWidth))
        {
            _lastHierarchyWidth = currentWidth;
            UpdateAllShelves();
        }
    }

    /// <summary>
    /// Reacts to changes in the hierarchy (e.g. manual name changes) and updates all shelf names.
    /// </summary>
    private static void OnHierarchyChanged()
    {
        UpdateAllShelves();
    }

    /// <summary>
    /// Scans the scene for objects with the ShelfIdentifier component and updates their names.
    /// If no shelves are found, it unsubscribes from the events.
    /// </summary>
    private static void UpdateAllShelves()
    {
        ShelfIdentifier[] shelfIdentifiers = Object.FindObjectsByType<ShelfIdentifier>(FindObjectsSortMode.None);
        if (shelfIdentifiers.Length == 0)
        {
            if (_eventsSubscribed)
            {
                EditorApplication.update -= UpdateShelvesOnResize;
                EditorApplication.hierarchyChanged -= OnHierarchyChanged;
                _eventsSubscribed = false;
            }
            return;
        }

        foreach (ShelfIdentifier identifier in shelfIdentifiers)
        {
            if (identifier != null)
            {
                // Use the stored base name
                identifier.gameObject.name = FormatShelfName(identifier.baseName);
            }
        }
    }

    /// <summary>
    /// Formats the shelf name by adding a number of symbols on both sides,
    /// based on the available width of the Hierarchy window.
    /// </summary>
   private static string FormatShelfName(string baseName)
{
    try
    {
        GUIStyle style = EditorStyles.label;
        if (style == null)
        {
            Debug.LogWarning("EditorStyles.label is null, set new GUIStyle.");
            style = new GUIStyle();
        }
        
        float hierarchyWidth = GetHierarchyWindowWidth();
        float availableWidth = Mathf.Max(hierarchyWidth - Margin, 0f);

        float baseWidth = style.CalcSize(new GUIContent(baseName)).x;
        float symbolWidth = style.CalcSize(new GUIContent(ShelfSymbol)).x;
        
        if (symbolWidth <= 0 || availableWidth <= baseWidth)
            return baseName;

        float remainingWidth = availableWidth - baseWidth;
        int totalSymbols = Mathf.FloorToInt(remainingWidth / symbolWidth);
        
        int leftCount = totalSymbols / 2;
        int rightCount = totalSymbols - leftCount;
        
        string result = new string(ShelfSymbol[0], leftCount) + baseName + new string(ShelfSymbol[0], rightCount);
        float resultWidth = style.CalcSize(new GUIContent(result)).x;
        
        bool added = true;
        while (added)
        {
            added = false;
            string testLeft = new string(ShelfSymbol[0], leftCount + 1) + baseName + new string(ShelfSymbol[0], rightCount);
            if (style.CalcSize(new GUIContent(testLeft)).x <= availableWidth)
            {
                leftCount++;
                resultWidth = style.CalcSize(new GUIContent(testLeft)).x;
                added = true;
            }

            string testRight = new string(ShelfSymbol[0], leftCount) + baseName + new string(ShelfSymbol[0], rightCount + 1);
            if (style.CalcSize(new GUIContent(testRight)).x <= availableWidth)
            {
                rightCount++;
                resultWidth = style.CalcSize(new GUIContent(testRight)).x;
                added = true;
            }
        }
        
        while (resultWidth > availableWidth && (leftCount > 0 || rightCount > 0))
        {
            if (leftCount >= rightCount && leftCount > 0)
            {
                string test = new string(ShelfSymbol[0], leftCount - 1) + baseName + new string(ShelfSymbol[0], rightCount);
                if (style.CalcSize(new GUIContent(test)).x <= availableWidth)
                {
                    leftCount--;
                    resultWidth = style.CalcSize(new GUIContent(test)).x;
                    continue;
                }
            }
            if (rightCount > 0)
            {
                string test = new string(ShelfSymbol[0], leftCount) + baseName + new string(ShelfSymbol[0], rightCount - 1);
                if (style.CalcSize(new GUIContent(test)).x <= availableWidth)
                {
                    rightCount--;
                    resultWidth = style.CalcSize(new GUIContent(test)).x;
                    continue;
                }
            }
            break;
        }

        return new string(ShelfSymbol[0], leftCount) + baseName + new string(ShelfSymbol[0], rightCount);
    }
    catch (System.Exception ex)
    {
        Debug.LogWarning("B³¹d w FormatShelfName: " + ex.Message + "\nZwracam bazow¹ nazwê.");
        return baseName;
    }
}


    /// <summary>
    /// Gets the width of the Hierarchy window – tries to find the active (focused) window,
    /// and if that fails, returns the width of the first found window.
    /// </summary>
    private static float GetHierarchyWindowWidth()
    {
        System.Type hierarchyType = System.Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor");
        if (hierarchyType != null)
        {
            var windows = Resources.FindObjectsOfTypeAll(hierarchyType);
            if (windows is { Length: > 0 })
            {
                // Look for the active (focused) Hierarchy window
                foreach (var win in windows)
                {
                    if (win is EditorWindow window && window == EditorWindow.focusedWindow)
                        return window.position.width;
                }
                // If no active window is found, return the first one found
                if (windows[0] is EditorWindow first)
                    return first.position.width;
            }
        }
        return 400f; // Default value if the window cannot be obtained
    }
}
