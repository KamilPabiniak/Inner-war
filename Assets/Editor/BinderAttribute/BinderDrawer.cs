using UnityEditor;
using UnityEngine;

namespace BinderAttribute
{
    [CustomPropertyDrawer(typeof(Binder))]
    public class BinderDrawer : DecoratorDrawer
    {

        public override void OnGUI(Rect position)
        {
            Binder styledHeader = (Binder)attribute;
            string prefsKey = $"Binder_{styledHeader.header}";
            bool isFoldout = styledHeader.foldAll;
            bool isExpanded = EditorPrefs.GetBool(prefsKey, true);

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

            if (isFoldout)
            {
                Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, GUIContent.none, true);
                // Rysujemy etykietê tylko jeœli nie mamy "uciszonego" nag³ówka
                EditorGUI.LabelField(foldoutRect, styledHeader.header, style);
                EditorPrefs.SetBool(prefsKey, isExpanded);
            }
            else
            {
                // Dla Binder bez foldAll – rysujemy nag³ówek, chyba ¿e jest uciszony
                Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(rect, styledHeader.header, style);
            }

            GUI.contentColor = originalColor;
        }

        public override float GetHeight()
        {
            Binder styledHeader = (Binder)attribute;
            return EditorGUIUtility.singleLineHeight + styledHeader.topSpace + styledHeader.bottomSpace;
        }
    }
}
