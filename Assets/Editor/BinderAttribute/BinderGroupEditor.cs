using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace BinderAttribute
{
    [CustomEditor(typeof(Object), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class BinderGroupEditor : Editor
    {
        // U¿ywamy List zamiast Dictionary, aby zachowaæ kolejnoœæ wstawiania.
        List<FoldGroupCache> cacheGroups = new List<FoldGroupCache>();
        List<SerializedProperty> ungroupedProps = new List<SerializedProperty>();
        List<SerializedProperty> allProps = new List<SerializedProperty>();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Za ka¿dym razem przeliczamy cache – wszelkie zmiany s¹ widoczne
            Setup();

            // Rysujemy zawsze m_Script (jeœli istnieje)
            DrawScriptField();
            
            // Rysujemy pozosta³e (niezgrupowane) w³aœciwoœci
            foreach (var prop in ungroupedProps)
            {
                EditorGUILayout.PropertyField(prop, true);
            }

            // Rysujemy zgrupowane w³aœciwoœci zgodnie z kolejnoœci¹ deklaracji
            foreach (var group in cacheGroups)
            {
                DrawGroup(group);
            }


            serializedObject.ApplyModifiedProperties();
        }

        void DrawScriptField()
        {
            if (allProps.Count > 0 && allProps[0].propertyPath == "m_Script")
            {
                EditorGUILayout.PropertyField(allProps[0], true);
                EditorGUILayout.Space();
            }
        }

        void DrawGroup(FoldGroupCache group)
        {
            string prefsKey = "Binder_" + group.binder.header + "_" + target.GetInstanceID();
            bool isExpanded = EditorPrefs.GetBool(prefsKey, group.binder.foldAll ? true : false);

            ColorUtility.TryParseHtmlString(group.binder.colorHex, out Color headerColor);
            Color originalColor = GUI.contentColor;
            GUI.contentColor = headerColor;

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = group.binder.fontSize,
                fontStyle = group.binder.fontStyle,
                alignment = group.binder.alignment
            };

            GUILayout.Space(group.binder.topSpace);
            isExpanded = EditorGUILayout.Foldout(isExpanded, group.binder.header, true, style);
            EditorPrefs.SetBool(prefsKey, isExpanded);
            GUILayout.Space(group.binder.bottomSpace);
            GUI.contentColor = originalColor;

            if (isExpanded)
            {
                EditorGUI.indentLevel++;
                foreach (var prop in group.groupProps)
                {
                    EditorGUILayout.PropertyField(prop, true);
                }
                EditorGUI.indentLevel--;
            }
        }

        void Setup()
        {
            cacheGroups.Clear();
            ungroupedProps.Clear();
            allProps.Clear();

            // Pobierz wszystkie serialized properties w kolejnoœci deklaracji
            SerializedProperty prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    allProps.Add(prop.Copy());
                }
                while (prop.NextVisible(false));
            }

            int i = 0;
            while (i < allProps.Count)
            {
                SerializedProperty currentProp = allProps[i];
                if (currentProp.propertyPath == "m_Script")
                {
                    i++;
                    continue;
                }

                FieldInfo fi = GetFieldInfo(currentProp);
                if (fi != null)
                {
                    Binder[] binders = fi.GetCustomAttributes(typeof(Binder), false) as Binder[];
                    if (binders != null && binders.Length > 0)
                    {
                        // Jeœli pierwszy Binder ma foldAll = true – traktujemy pole jako nag³ówek grupy
                        if (binders[0].foldAll)
                        {
                            FoldGroupCache groupCache = new FoldGroupCache();
                            groupCache.binder = binders[0];
                            groupCache.headerProp = currentProp.Copy();
                            
                            // Jeœli pole ma wiêcej ni¿ jeden atrybut Binder, dodajemy je te¿ do listy w³aœciwoœci grupy
                            if (binders.Length > 1)
                            {
                                groupCache.groupProps.Add(currentProp.Copy());
                            }
                            
                            cacheGroups.Add(groupCache);
                            i++; // Pomijamy nag³ówek

                            // Dodajemy kolejne pola do grupy a¿ do napotkania StopFold lub kolejnego Binder z foldAll = true
                            while (i < allProps.Count)
                            {
                                FieldInfo nextFi = GetFieldInfo(allProps[i]);
                                if (nextFi != null)
                                {
                                    if (nextFi.GetCustomAttributes(typeof(StopFold), false).Length > 0)
                                        break;

                                    Binder[] nextBinders = nextFi.GetCustomAttributes(typeof(Binder), false) as Binder[];
                                    if (nextBinders != null && nextBinders.Length > 0)
                                    {
                                        if (nextBinders[0].foldAll)
                                            break;
                                    }
                                }
                                groupCache.groupProps.Add(allProps[i].Copy());
                                i++;
                            }
                            continue;
                        }
                    }
                }
                // Jeœli w³aœciwoœæ nie nale¿y do ¿adnej grupy, dodajemy j¹ do listy niezgrupowanych
                ungroupedProps.Add(currentProp.Copy());
                i++;
            }
        }

        FieldInfo GetFieldInfo(SerializedProperty property)
        {
            System.Type type = target.GetType();
            FieldInfo field = type.GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field;
        }

        class FoldGroupCache
        {
            public Binder binder;
            public SerializedProperty headerProp;
            public List<SerializedProperty> groupProps = new List<SerializedProperty>();
        }
    }
}
