using UnityEditor;
using UnityEngine;

namespace Header_Upgrade
{
    [CustomPropertyDrawer(typeof(AdvancedHeader))]
    public class AdvancedHeaderDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            AdvancedHeader styledHeader = (AdvancedHeader)attribute;
            string prefsKey = $"AdvancedHeader_{styledHeader.header}";

            if (!EditorPrefs.HasKey(prefsKey))
                EditorPrefs.SetBool(prefsKey, true);

            ColorUtility.TryParseHtmlString(styledHeader.colorHex, out Color headerColor);
            Color originalColor = GUI.contentColor;
            GUI.contentColor = headerColor;

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = styledHeader.fontSize,
                fontStyle = styledHeader.fontStyle,
                alignment = styledHeader.alignment
            };

            position.y += styledHeader.topSpace;
            
            EditorGUI.LabelField(position, styledHeader.header, style);
            
            GUI.contentColor = originalColor;
        }

        public override float GetHeight()
        {
            AdvancedHeader styledHeader = (AdvancedHeader)attribute;
            return EditorGUIUtility.singleLineHeight + styledHeader.topSpace + styledHeader.bottomSpace;
        }
    }
}