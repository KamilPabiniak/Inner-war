using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(AdvancedHeader))]
public class AdvancedHeaderDrawer : DecoratorDrawer
{
    public override void OnGUI(Rect position)
    {
        AdvancedHeader styledHeader = (AdvancedHeader)attribute;
        Color originalColor = GUI.contentColor;
        GUI.contentColor = new Color(styledHeader.color.r, styledHeader.color.g, styledHeader.color.b);
    
        GUIStyle style = new GUIStyle(EditorStyles.label)
        {
            fontSize = styledHeader.fontSize,
            fontStyle = styledHeader.fontStyle,
            alignment = styledHeader.alignment 
        };

        position.y += styledHeader.topSpace;
        EditorGUI.LabelField(position, styledHeader.header20, style);
        GUI.contentColor = originalColor;
    }

    public override float GetHeight()
    {
        AdvancedHeader styledHeader = (AdvancedHeader)attribute;
        return EditorGUIUtility.singleLineHeight + styledHeader.topSpace + styledHeader.bottomSpace;
    }
}
