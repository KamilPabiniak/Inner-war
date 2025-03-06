using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class SceneShelfCreator
{
    private const float Margin = 50f;
    private const string DefaultShelfName = "New Shelf";
    private const double DebounceThreshold = 0.1; // Delay threshold (in seconds)
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
        ShelfIdentifier identifier = CreateShelfWithDefaultValues();
        // Format the shelf name right after creation.
        string formattedName = FormatShelfName(identifier);
        _lastHierarchyWidth = GetHierarchyWindowWidth();
        EnsureEventsSubscribed();
    }

    private static ShelfIdentifier CreateShelfWithDefaultValues()
    {
        GameObject shelfObject = new GameObject(DefaultShelfName)
        {
            transform = { hideFlags = HideFlags.HideInInspector }
        };
        ShelfIdentifier identifier = shelfObject.AddComponent<ShelfIdentifier>();
        identifier.baseName = DefaultShelfName;
        identifier.useTextColor = false;
        identifier.useBackgroundColor = false;
        Undo.RegisterCreatedObjectUndo(shelfObject, "Create Shelf");
        Selection.activeGameObject = shelfObject;
        return identifier;
    }

    [MenuItem("GameObject/Create Shelf", true)]
    private static bool ValidateCreateShelfFromMenu()
    {
        return !Application.isPlaying;
    }

    private static void EnsureEventsSubscribed()
    {
        if (!_eventsSubscribed && Object.FindObjectsByType<ShelfIdentifier>(FindObjectsSortMode.None).Length > 0)
        {
            EditorApplication.update += UpdateShelvesOnResize;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            _eventsSubscribed = true;
        }
    }

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

    private static void OnHierarchyChanged()
    {
        UpdateAllShelves();
    }

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
                identifier.gameObject.name = FormatShelfName(identifier);
            }
        }
    }

    /// <summary>
    /// Formats the shelf name by adding special symbols on both sides (depending on settings)
    /// and wrapping text/symbols with rich text tags.
    /// </summary>
    private static string FormatShelfName(ShelfIdentifier shelf)
    {
        string baseName = shelf.baseName;
        // Safely retrieve a label style.
        GUIStyle style = null;
        try
        {
            style = EditorStyles.label;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Nie uda³o siê pobraæ EditorStyles.label: " + ex);
        }
        style ??= GUI.skin.label;
        if (style == null)
        {
            Debug.LogWarning("Zarówno EditorStyles.label, jak i GUI.skin.label s¹ null. Inicjalizujê nowy GUIStyle.");
            style = new GUIStyle();
        }

        float hierarchyWidth = GetHierarchyWindowWidth();
        float availableWidth = Mathf.Max(hierarchyWidth - Margin, 0f);
        float baseWidth = style.CalcSize(new GUIContent(baseName)).x;

        // If available width is less than or equal to the base text width, return the base text (with gradient or color if enabled)
        if (availableWidth <= baseWidth)
        {
            return shelf.useTextGradient
                ? ApplyGradientToString(baseName, shelf.textGradientColors, shelf.textGradientDirection)
                : (shelf.useTextColor
                    ? $"<color=#{ColorUtility.ToHtmlStringRGBA(shelf.textColor)}>{baseName}</color>"
                    : baseName);
        }

        // Determine special symbols based on the type.
        char leftSymbol, rightSymbol;
        switch (shelf.specialSymbolType)
        {
            case ShelfIdentifier.SpecialSymbolType.Default:
                leftSymbol = '-'; rightSymbol = '-';
                break;
            case ShelfIdentifier.SpecialSymbolType.Percent:
                leftSymbol = '%'; rightSymbol = '%';
                break;
            case ShelfIdentifier.SpecialSymbolType.Ampersand:
                leftSymbol = '&'; rightSymbol = '&';
                break;
            case ShelfIdentifier.SpecialSymbolType.At:
                leftSymbol = '@'; rightSymbol = '@';
                break;
            case ShelfIdentifier.SpecialSymbolType.Parentheses:
                leftSymbol = '('; rightSymbol = ')';
                break;
            case ShelfIdentifier.SpecialSymbolType.Tilde:
                leftSymbol = '~'; rightSymbol = '~';
                break;
            case ShelfIdentifier.SpecialSymbolType.Custom:
                if (!string.IsNullOrEmpty(shelf.customSymbol))
                {
                    leftSymbol = shelf.customSymbol[0];
                    rightSymbol = leftSymbol;
                }
                else
                {
                    leftSymbol = '-'; rightSymbol = '-';
                }
                break;
            default:
                leftSymbol = '-'; rightSymbol = '-';
                break;
        }

        string symbolForCalc = new string(leftSymbol, 1);
        float symbolWidth = style.CalcSize(new GUIContent(symbolForCalc)).x;
        if (symbolWidth <= 0 || availableWidth <= baseWidth)
            return baseName;

        float remainingWidth = availableWidth - baseWidth;
        int totalSymbols = Mathf.FloorToInt(remainingWidth / symbolWidth);

        int leftCount, rightCount;
        if (shelf.displayLeftSymbols && shelf.displayRightSymbols)
        {
            // Ensure that the initial leftCount is not negative.
            leftCount = Mathf.Max((totalSymbols / 2) - 2, 0);
            rightCount = totalSymbols - leftCount;
            bool added = true;
            float resultWidth = style.CalcSize(new GUIContent(new string(leftSymbol, leftCount) + baseName + new string(rightSymbol, rightCount))).x;
            // Increase symbols while the result width is less than the available width.
            while (added)
            {
                added = false;
                if (shelf.displayLeftSymbols)
                {
                    string testLeft = new string(leftSymbol, leftCount + 1) + baseName + new string(rightSymbol, rightCount);
                    if (style.CalcSize(new GUIContent(testLeft)).x <= availableWidth)
                    {
                        leftCount++;
                        resultWidth = style.CalcSize(new GUIContent(testLeft)).x;
                        added = true;
                    }
                }
                if (shelf.displayRightSymbols)
                {
                    string testRight = new string(leftSymbol, leftCount) + baseName + new string(rightSymbol, rightCount + 1);
                    if (style.CalcSize(new GUIContent(testRight)).x <= availableWidth)
                    {
                        rightCount++;
                        resultWidth = style.CalcSize(new GUIContent(testRight)).x;
                        added = true;
                    }
                }
            }
            // Decrease symbols if the result width exceeds available width.
            while (resultWidth > availableWidth && (leftCount > 0 || rightCount > 0))
            {
                if (shelf.displayLeftSymbols && leftCount >= rightCount && leftCount > 0)
                {
                    string test = new string(leftSymbol, leftCount - 1) + baseName + new string(rightSymbol, rightCount);
                    if (style.CalcSize(new GUIContent(test)).x <= availableWidth)
                    {
                        leftCount--;
                        resultWidth = style.CalcSize(new GUIContent(test)).x;
                        continue;
                    }
                }
                if (shelf.displayRightSymbols && rightCount > 0)
                {
                    string test = new string(leftSymbol, leftCount) + baseName + new string(rightSymbol, rightCount - 1);
                    if (style.CalcSize(new GUIContent(test)).x <= availableWidth)
                    {
                        rightCount--;
                        resultWidth = style.CalcSize(new GUIContent(test)).x;
                        continue;
                    }
                }
                break;
            }
        }
        else if (shelf.displayLeftSymbols && !shelf.displayRightSymbols)
        {
            rightCount = 0;
            leftCount = Mathf.FloorToInt(totalSymbols * (shelf.reduceLeftSymbolsPercent / 100f));
        }
        else if (!shelf.displayLeftSymbols && shelf.displayRightSymbols)
        {
            leftCount = 0;
            rightCount = Mathf.FloorToInt(totalSymbols * (shelf.reduceRightSymbolsPercent / 100f));
        }
        else
        {
            return shelf.useTextGradient
                ? ApplyGradientToString(baseName, shelf.textGradientColors, shelf.textGradientDirection)
                : (shelf.useTextColor
                    ? $"<color=#{ColorUtility.ToHtmlStringRGBA(shelf.textColor)}>{baseName}</color>"
                    : baseName);
        }

        // Apply text color or gradient to the base name.
        string coloredBaseName;
        if (shelf.useTextGradient)
            coloredBaseName = ApplyGradientToString(baseName, shelf.textGradientColors, shelf.textGradientDirection);
        else if (shelf.useTextColor)
            coloredBaseName = $"<color=#{ColorUtility.ToHtmlStringRGBA(shelf.textColor)}>{baseName}</color>";
        else
            coloredBaseName = baseName;

        // Format left symbols.
        string leftSymbolsStr = "";
        if (shelf.displayLeftSymbols)
        {
            string temp = new string(leftSymbol, leftCount);
            if (shelf.useSymbolsGradient)
                leftSymbolsStr = ApplyGradientToString(temp, shelf.symbolsGradientColors, shelf.symbolsGradientDirection);
            else if (shelf.colorSymbolsSame && shelf.useTextColor)
            {
                string hexText = ColorUtility.ToHtmlStringRGBA(shelf.textColor);
                leftSymbolsStr = $"<color=#{hexText}>{temp}</color>";
            }
            else if (shelf.colorSymbolsDifferent)
            {
                string hexSymbols = ColorUtility.ToHtmlStringRGBA(shelf.symbolsColor);
                leftSymbolsStr = $"<color=#{hexSymbols}>{temp}</color>";
            }
            else
            {
                leftSymbolsStr = temp;
            }
        }

        // Format right symbols.
        string rightSymbolsStr = "";
        if (shelf.displayRightSymbols)
        {
            string temp = new string(rightSymbol, rightCount);
            if (shelf.useSymbolsGradient)
                rightSymbolsStr = ApplyGradientToString(temp, shelf.symbolsGradientColors, shelf.symbolsGradientDirection);
            else if (shelf.colorSymbolsSame && shelf.useTextColor)
            {
                string hexText = ColorUtility.ToHtmlStringRGBA(shelf.textColor);
                rightSymbolsStr = $"<color=#{hexText}>{temp}</color>";
            }
            else if (shelf.colorSymbolsDifferent)
            {
                string hexSymbols = ColorUtility.ToHtmlStringRGBA(shelf.symbolsColor);
                rightSymbolsStr = $"<color=#{hexSymbols}>{temp}</color>";
            }
            else
            {
                rightSymbolsStr = temp;
            }
        }

        return leftSymbolsStr + coloredBaseName + rightSymbolsStr;
    }

    /// <summary>
    /// Applies a gradient to each character of the input string.
    /// </summary>
    private static string ApplyGradientToString(string input, Color[] gradientColors, ShelfIdentifier.GradientDirection gradientDirection)
    {
        if (input.Length == 0)
            return input;

        string result = "";
        for (int i = 0; i < input.Length; i++)
        {
            float t;
            if (gradientDirection == ShelfIdentifier.GradientDirection.CenterOutward)
            {
                float center = (input.Length - 1) / 2f;
                t = (center > 0f) ? Mathf.Abs(i - center) / center : 0f;
            }
            else // LeftToRight
            {
                t = (input.Length > 1) ? (float)i / (input.Length - 1) : 0f;
            }
            Color col = EvaluateGradient(gradientColors, t);
            string hex = ColorUtility.ToHtmlStringRGBA(col);
            result += $"<color=#{hex}>{input[i]}</color>";
        }
        return result;
    }

    /// <summary>
    /// Evaluates a multi-stop gradient given an array of colors and a normalized t (0-1).
    /// </summary>
    private static Color EvaluateGradient(Color[] colors, float t)
    {
        if (colors == null || colors.Length == 0)
            return Color.white;
        if (colors.Length == 1)
            return colors[0];
        t = Mathf.Clamp01(t);
        float scaledT = t * (colors.Length - 1);
        int index = Mathf.FloorToInt(scaledT);
        if (index >= colors.Length - 1)
            return colors[colors.Length - 1];
        float localT = scaledT - index;
        return Color.Lerp(colors[index], colors[index + 1], localT);
    }

    /// <summary>
    /// Retrieves the width of the hierarchy window.
    /// </summary>
    private static float GetHierarchyWindowWidth()
    {
        System.Type hierarchyType = System.Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor");
        if (hierarchyType != null)
        {
            var windows = Resources.FindObjectsOfTypeAll(hierarchyType);
            if (windows is { Length: > 0 })
            {
                foreach (var win in windows)
                {
                    if (win is EditorWindow window && window == EditorWindow.focusedWindow)
                        return window.position.width;
                }
                if (windows[0] is EditorWindow first)
                    return first.position.width;
            }
        }
        return 400f;
    }
}
