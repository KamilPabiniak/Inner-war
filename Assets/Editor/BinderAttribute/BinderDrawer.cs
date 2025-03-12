using UnityEditor;
using UnityEngine;

namespace BinderAttribute
{
    [CustomPropertyDrawer(typeof(Binder))]
    public class BinderDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var binder = (Binder)attribute;
            if (binder.foldAll)
                return;

            ColorUtility.TryParseHtmlString(binder.colorHex, out Color headerColor);
            var originalColor = GUI.contentColor;
            GUI.contentColor = headerColor;

            var style = GetStyle(binder);
            style.wordWrap = true; // Enable word wrapping

            // Apply top spacing
            position.y += binder.topSpace;

            // Calculate dynamic height for the header text based on available width.
            float headerHeight = style.CalcHeight(new GUIContent(binder.header), position.width);
            var rect = new Rect(position.x, position.y, position.width, headerHeight);
            EditorGUI.LabelField(rect, binder.header, style);

            GUI.contentColor = originalColor;
        }

        public static GUIStyle GetStyle(Binder binder)
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = binder.fontSize,
                fontStyle = binder.fontStyle,
                alignment = binder.alignment
            };
            if (ColorUtility.TryParseHtmlString(binder.colorHex, out Color headerColor))
            {
                style.normal.textColor = headerColor;
            }
            return style;
        }

        public override float GetHeight()
        {
            var binder = (Binder)attribute;
            if (binder.foldAll)
            {
                return 0f;
            }
            
            var style = GetStyle(binder);
            style.wordWrap = true; // Ensure word wrap is enabled for height calculation
            
            // Use current view width as an approximation for available width.
            float availableWidth = EditorGUIUtility.currentViewWidth;
            float headerHeight = style.CalcHeight(new GUIContent(binder.header), availableWidth);
            return binder.topSpace + headerHeight + binder.bottomSpace;
        }
    }
}
