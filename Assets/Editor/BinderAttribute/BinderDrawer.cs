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

            position.y += binder.topSpace;
            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
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
            return EditorGUIUtility.singleLineHeight + binder.topSpace + binder.bottomSpace;
        }
    }
}