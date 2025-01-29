using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class AdvancedHeaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;
        bool visible = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;
            FieldInfo field = target.GetType().GetField(property.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                AdvancedHeader header = field.GetCustomAttribute<AdvancedHeader>();
                if (header != null && header.hideMe && AdvancedHeaderDrawer.ShouldHideMe(header.header))
                {
                    visible = false;
                }
                else
                {
                    visible = true;
                }
            }

            if (visible)
            {
                EditorGUILayout.PropertyField(property, true);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
