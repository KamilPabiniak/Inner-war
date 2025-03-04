using UnityEditor;
using UnityEngine;

namespace BinderAttribute
{
    [CustomPropertyDrawer(typeof(Binder))]
    public class BinderDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            Binder binder = (Binder)attribute;
            // Jeœli Binder jest przeznaczony do grupowania (foldAll), nie rysujemy nag³ówka w polu.
            if (binder.foldAll)
                return;

            ColorUtility.TryParseHtmlString(binder.colorHex, out Color headerColor);
            Color originalColor = GUI.contentColor;
            GUI.contentColor = headerColor;

            GUIStyle style = GetStyle(binder);

            position.y += binder.topSpace;

            // Rysujemy etykietê przy polu (nie ma foldout, bo nie jest grupowany)
            Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
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
            return style;
        }

        public override float GetHeight()
        {
            Binder binder = (Binder)attribute;
            if (binder.foldAll)
            {
                // Jeœli pole jest przeznaczone tylko do grupowania, zwracamy 0 – nie rysujemy nag³ówka.
                return 0f;
            }
            return EditorGUIUtility.singleLineHeight + binder.topSpace + binder.bottomSpace;
        }
    }
}