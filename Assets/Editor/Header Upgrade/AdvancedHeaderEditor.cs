using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(Object), true, isFallback = true)]
[CanEditMultipleObjects]
public class AdvancedHeaderEditor : Editor
{
    Dictionary<string, CacheFoldProp> cacheFolds = new Dictionary<string, CacheFoldProp>();
    List<SerializedProperty> props = new List<SerializedProperty>();
    bool initialized;

    void OnEnable()
    {
        initialized = false;
    }

    void OnDisable()
    {
        if (target != null)
        {
            foreach (var c in cacheFolds)
            {
                EditorPrefs.SetBool($"{c.Value.atr.header}{c.Value.props[0].name}{target.GetInstanceID()}", c.Value.expanded);
                c.Value.Dispose();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Setup();

        if (props.Count == 0)
        {
            DrawDefaultInspector();
            return;
        }

        Header();
        Body();

        serializedObject.ApplyModifiedProperties();
    }

    void Header()
    {
        using (new EditorGUI.DisabledScope("m_Script" == props[0].propertyPath))
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(props[0], true);
            EditorGUILayout.Space();
        }
    }

    void Body()
    {
        foreach (var pair in cacheFolds)
        {
            if (!pair.Value.atr.isFoldable)
            {
                foreach (var prop in pair.Value.props)
                    EditorGUILayout.PropertyField(prop, true);
            }
            else
            {
                pair.Value.expanded = EditorGUILayout.Foldout(pair.Value.expanded, pair.Value.atr.header, true);
                if (pair.Value.expanded)
                {
                    EditorGUI.indentLevel++;
                    foreach (var prop in pair.Value.props)
                        EditorGUILayout.PropertyField(prop, true);
                    EditorGUI.indentLevel--;
                }
            }
        }

        for (int i = 1; i < props.Count; i++)
        {
            EditorGUILayout.PropertyField(props[i], true);
        }
    }

    void Setup()
    {
        if (initialized) return;

        List<FieldInfo> objectFields;
        int length = EditorTypes.Get(target, out objectFields);

        for (int i = 0; i < length; i++)
        {
            var fold = Attribute.GetCustomAttribute(objectFields[i], typeof(AdvancedHeader)) as AdvancedHeader;
            if (fold == null) continue;

            if (!cacheFolds.TryGetValue(fold.header, out var c))
            {
                bool expanded = EditorPrefs.GetBool($"{fold.header}{objectFields[i].Name}{target.GetInstanceID()}", false);
                cacheFolds.Add(fold.header, new CacheFoldProp { atr = fold, expanded = expanded });
            }
            cacheFolds[fold.header].props.Add(serializedObject.FindProperty(objectFields[i].Name));
        }

        var property = serializedObject.GetIterator();
        if (property.NextVisible(true))
        {
            do { props.Add(property.Copy()); } while (property.NextVisible(false));
        }
        
        initialized = true;
    }

    class CacheFoldProp
    {
        public AdvancedHeader atr;
        public List<SerializedProperty> props = new List<SerializedProperty>();
        public bool expanded;

        public void Dispose()
        {
            props.Clear();
            atr = null;
        }
    }
}

public static class EditorTypes
{
    public static Dictionary<int, List<FieldInfo>> fields = new Dictionary<int, List<FieldInfo>>();

    public static int Get(Object target, out List<FieldInfo> objectFields)
    {
        var t = target.GetType();
        var hash = t.GetHashCode();

        if (!fields.TryGetValue(hash, out objectFields))
        {
            objectFields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).ToList();
            fields.Add(hash, objectFields);
        }

        return objectFields.Count;
    }
}
