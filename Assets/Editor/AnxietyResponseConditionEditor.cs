#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Anxiety;

[CustomEditor(typeof(AnxietyResponseCondition))]
public class AnxietyResponseConditionEditor : Editor
{
    private SerializedProperty _levelAudios;

    private void OnEnable()
    {
        _levelAudios = serializedObject.FindProperty("levelAudios");
        EnsureSixLevels();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EnsureSixLevels();

        DrawPropertiesExcluding(serializedObject, "m_Script", "levelAudios");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Audio Settings", EditorStyles.boldLabel);

        for (int i = 0; i < _levelAudios.arraySize; i++)
        {
            var element = _levelAudios.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("fearLevel").intValue = i;
            element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, $"Level {i} Audio", true);
            if (element.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(element.FindPropertyRelative("clips"), new GUIContent("Clips"), true);
                EditorGUILayout.PropertyField(element.FindPropertyRelative("allowReplay"), new GUIContent("Allow Replay"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("playSequentially"), new GUIContent("Play Sequentially"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space(4);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void EnsureSixLevels()
    {
        if (_levelAudios.arraySize != 6)
        {
            _levelAudios.arraySize = 6;
            for (int i = 0; i < 6; i++)
            {
                var elem = _levelAudios.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("fearLevel").intValue = i;
                elem.FindPropertyRelative("hasPlayed").boolValue = false;
                elem.FindPropertyRelative("nextIndex").intValue = 0;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
