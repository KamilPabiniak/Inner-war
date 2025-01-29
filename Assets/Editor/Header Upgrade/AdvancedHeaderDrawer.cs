using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(AdvancedHeader))]
public class AdvancedHeaderDrawer : DecoratorDrawer
{
    private static Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();
    private static string lastFoldableHeader = null;
    private static bool lastFoldoutState = true;

    public override void OnGUI(Rect position)
    {
        AdvancedHeader styledHeader = (AdvancedHeader)attribute;
        string key = styledHeader.header;

        if (!foldoutStates.ContainsKey(key))
            foldoutStates[key] = true;

        Color originalColor = GUI.contentColor;
        GUI.contentColor = styledHeader.color;

        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout)
        {
            fontSize = styledHeader.fontSize,
            fontStyle = styledHeader.fontStyle,
            alignment = styledHeader.alignment
        };

        GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = styledHeader.fontSize,
            fontStyle = styledHeader.fontStyle,
            alignment = styledHeader.alignment
        };

        position.y += styledHeader.topSpace;

        if (styledHeader.foldable)
        {
            foldoutStates[key] = EditorGUI.Foldout(position, foldoutStates[key], styledHeader.header, true, foldoutStyle);
            lastFoldableHeader = key;
            lastFoldoutState = foldoutStates[key];
        }
        else
        {
            EditorGUI.LabelField(position, styledHeader.header, labelStyle);
        }

        GUI.contentColor = originalColor;
    }

    public override float GetHeight()
    {
        AdvancedHeader styledHeader = (AdvancedHeader)attribute;
        return EditorGUIUtility.singleLineHeight + styledHeader.topSpace + styledHeader.bottomSpace;
    }

    public static bool ShouldHideMe(string header)
    {
        return lastFoldableHeader != null && !lastFoldoutState;
    }
}