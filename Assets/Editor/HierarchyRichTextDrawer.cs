using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class HierarchyRichTextDrawer
{
    static HierarchyRichTextDrawer()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
    }
    
    private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go == null)
            return;
            
        ShelfIdentifier shelf = go.GetComponent<ShelfIdentifier>();
        if (shelf != null)
        {
            // Prepare a GUIStyle with richText enabled.
            GUIStyle richStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true
            };

            // Set alignment based on which symbol options are enabled.
            if (shelf.displayLeftSymbols && shelf.displayRightSymbols)
            {
                richStyle.alignment = TextAnchor.MiddleCenter;
            }
            else if (!shelf.displayLeftSymbols && shelf.displayRightSymbols)
            {
                richStyle.alignment = TextAnchor.MiddleRight;
            }
            else if (shelf.displayLeftSymbols && !shelf.displayRightSymbols)
            {
                richStyle.alignment = TextAnchor.MiddleLeft;
            }
            else // both disabled – use user-chosen alignment
            {
                switch (shelf.textAlignment)
                {
                    case ShelfIdentifier.TextAlignmentOption.Left:
                        richStyle.alignment = TextAnchor.MiddleLeft;
                        break;
                    case ShelfIdentifier.TextAlignmentOption.Center:
                        richStyle.alignment = TextAnchor.MiddleCenter;
                        break;
                    case ShelfIdentifier.TextAlignmentOption.Right:
                        richStyle.alignment = TextAnchor.MiddleRight;
                        break;
                }
            }

            // Add extra right padding if text is right-aligned to prevent clipping.
            if (richStyle.alignment == TextAnchor.MiddleRight)
            {
                richStyle.padding = new RectOffset(0, 22, 0, 0);
            }

            // Calculate the formatted text's width.
            float calculatedWidth = richStyle.CalcSize(new GUIContent(go.name)).x;
            float fullRowWidth = EditorGUIUtility.currentViewWidth - selectionRect.x;
            // If any of the symbol options is disabled, extend background to the full width.
            float backgroundWidth = (shelf.displayLeftSymbols && shelf.displayRightSymbols) ? calculatedWidth : fullRowWidth;
            
            Rect textRect = new Rect(selectionRect.x, selectionRect.y, backgroundWidth, selectionRect.height);

            // Check if the object is selected.
            bool isSelected = Selection.objects.Contains(go);
            if (isSelected)
            {
                // Draw a rectangle filled with Unity's selection color.
                Color unitySelectionColor = EditorGUIUtility.isProSkin 
                    ? new Color(0.24f, 0.48f, 0.90f, 1f) 
                    : new Color(0.24f, 0.48f, 0.90f, 1f);
                EditorGUI.DrawRect(textRect, unitySelectionColor);
            }
            else
            {
                // Determine background colors.
                Color[] bgColors;
                if (shelf.useBackgroundGradient)
                {
                    bgColors = shelf.backgroundGradientColors;
                }
                else if (shelf.useBackgroundColor)
                {
                    bgColors = new Color[] { shelf.backgroundColor };
                }
                else
                {
                    bgColors = new Color[] { EditorGUIUtility.isProSkin 
                        ? new Color(0.22f, 0.22f, 0.22f, 1f) 
                        : new Color(0.76f, 0.76f, 0.76f, 1f) };
                }
                Texture2D bgTexture = CreateRoundedRectTexture((int)textRect.width, (int)textRect.height, bgColors, shelf.backgroundCornerRadius, shelf.backgroundGradientDirection);
                GUI.DrawTexture(textRect, bgTexture);
            }

            // Finally, draw the label (rich text) within the textRect.
            EditorGUI.LabelField(textRect, go.name, richStyle);
        }
    }

    // Generates a texture with rounded corners and a horizontal gradient.
    private static Texture2D CreateRoundedRectTexture(int width, int height, Color[] colors, float cornerRadius, ShelfIdentifier.GradientDirection gradientDirection)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        tex.hideFlags = HideFlags.DontSave;
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Determine if the pixel is outside the rounded corner.
                bool inCorner = false;
                if (x < cornerRadius && y < cornerRadius)
                {
                    if (Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, cornerRadius)) > cornerRadius)
                        inCorner = true;
                }
                else if (x >= width - cornerRadius && y < cornerRadius)
                {
                    if (Vector2.Distance(new Vector2(x, y), new Vector2(width - cornerRadius, cornerRadius)) > cornerRadius)
                        inCorner = true;
                }
                else if (x < cornerRadius && y >= height - cornerRadius)
                {
                    if (Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, height - cornerRadius)) > cornerRadius)
                        inCorner = true;
                }
                else if (x >= width - cornerRadius && y >= height - cornerRadius)
                {
                    if (Vector2.Distance(new Vector2(x, y), new Vector2(width - cornerRadius, height - cornerRadius)) > cornerRadius)
                        inCorner = true;
                }

                float alpha = inCorner ? 0f : 1f;

                // Calculate t value based on gradient direction.
                float t;
                if (gradientDirection == ShelfIdentifier.GradientDirection.CenterOutward)
                {
                    float center = width / 2f;
                    t = (center > 0f) ? Mathf.Abs(x - center) / center : 0f;
                }
                else // LeftToRight
                {
                    t = (width > 1) ? (float)x / (width - 1) : 0f;
                }
                Color col = EvaluateGradient(colors, t);
                col.a *= alpha;
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        return tex;
    }

    // Evaluates a multi-stop gradient given an array of colors and a normalized t (0-1).
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
}
