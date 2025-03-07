#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Anxiety.Effects;
using System;
using System.Linq;

[CustomEditor(typeof(FearLevelProfile))]
public class FearLevelProfileEditor : Editor
{
    private FearLevelProfile _profile;

    private void OnEnable()
    {
        _profile = (FearLevelProfile)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        // Wyœwietlenie listy obecnych efektów z przyciskiem do usuwania
        if (_profile.effects != null && _profile.effects.Count > 0)
        {
            EditorGUILayout.LabelField("Current Effects:");
            // Iterujemy od koñca, by bezpiecznie usuwaæ elementy z listy
            for (int i = _profile.effects.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(_profile.effects[i].name);
                if (GUILayout.Button("Remove"))
                {
                    RemoveEffect(i);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Add new effect"))
        {
            GenericMenu menu = new GenericMenu();

            var effectTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(BaseFearEffect)) && !type.IsAbstract);

            foreach (Type t in effectTypes)
            {
                menu.AddItem(new GUIContent(t.Name), false, () => AddEffect(t));
            }
            menu.ShowAsContext();
        }
    }

    private void AddEffect(Type type)
    {
        BaseFearEffect newEffect = (BaseFearEffect)CreateInstance(type);
        newEffect.name = type.Name;

        AssetDatabase.AddObjectToAsset(newEffect, _profile);
        _profile.effects.Add(newEffect);

        EditorUtility.SetDirty(_profile);
        AssetDatabase.SaveAssets();
    }

    private void RemoveEffect(int index)
    {
        BaseFearEffect effectToRemove = _profile.effects[index];
        _profile.effects.RemoveAt(index);

        AssetDatabase.RemoveObjectFromAsset(effectToRemove);
        DestroyImmediate(effectToRemove, true);

        EditorUtility.SetDirty(_profile);
        AssetDatabase.SaveAssets();
    }
}
#endif
