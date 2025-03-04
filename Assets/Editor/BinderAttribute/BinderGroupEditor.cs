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
        // U¿ywamy List zamiast Dictionary, by zachowaæ kolejnoœæ deklaracji.
        List<FoldGroupCache> cacheGroups = new List<FoldGroupCache>();
        List<SerializedProperty> ungroupedProps = new List<SerializedProperty>();
        List<SerializedProperty> allProps = new List<SerializedProperty>();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Setup();

            // Rysujemy zawsze m_Script (jeœli istnieje)
            DrawScriptField();

            // Rysujemy niezgrupowane w³aœciwoœci
            foreach (var prop in ungroupedProps)
            {
                EditorGUILayout.PropertyField(prop, true);
            }

            // Rysujemy grupy (nag³ówek oraz zawartoœæ)
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
            // U¿ywamy stylu z BinderDrawer (nawet jeœli sam BinderDrawer nie rysuje nag³ówka przy foldAll)
            GUIStyle style = BinderDrawer.GetStyle(group.headerBinder);

            string prefsKey = "Binder_" + group.headerBinder.header + "_" + target.GetInstanceID();
            bool isExpanded = EditorPrefs.GetBool(prefsKey, group.headerBinder.foldAll ? true : false);

            // Rysujemy nag³ówek grupy – korzystamy z danych z BinderDrawer
            GUILayout.Space(group.headerBinder.topSpace);
            isExpanded = EditorGUILayout.Foldout(isExpanded, group.headerBinder.header, true, style);
            EditorPrefs.SetBool(prefsKey, isExpanded);
            GUILayout.Space(group.headerBinder.bottomSpace);

            if (isExpanded)
            {
                EditorGUI.indentLevel++;
                foreach (var prop in group.groupProps)
                {
                    // Jeœli dana w³aœciwoœæ to ta, która mia³a oba atrybuty (u¿ywana jako nag³ówek),
                    // to rysujemy j¹ z etykiet¹ pobran¹ z fieldBinder – zapobiegamy duplikacji nag³ówka.
                    GUIContent label = null;
                    if (group.fieldBinder != null && prop.propertyPath == group.headerProp.propertyPath)
                    {
                        label = new GUIContent(group.fieldBinder.header);
                    }
                    else
                    {
                        label = new GUIContent(prop.displayName);
                    }
                    EditorGUILayout.PropertyField(prop, label, true);
                }
                EditorGUI.indentLevel--;
            }
        }

        void Setup()
        {
            cacheGroups.Clear();
            ungroupedProps.Clear();
            allProps.Clear();

            // Pobieramy wszystkie serialized properties w kolejnoœci deklaracji
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
                        // Jeœli pierwszy Binder ma foldAll = true, traktujemy to pole jako nag³ówek grupy.
                        if (binders[0].foldAll)
                        {
                            FoldGroupCache groupCache = new FoldGroupCache();
                            groupCache.headerBinder = binders[0];
                            groupCache.headerProp = currentProp.Copy();
                            
                            // Jeœli na tym polu jest drugi Binder (przeznaczony do rysowania w³aœciwoœci),
                            // zapisujemy go i dodajemy w³aœciwoœæ do grupy.
                            if (binders.Length > 1)
                            {
                                groupCache.fieldBinder = binders[1];
                                groupCache.groupProps.Add(currentProp.Copy());
                            }
                            // W przeciwnym razie, pole s³u¿y wy³¹cznie jako nag³ówek – nie dodajemy go do listy.
                            
                            cacheGroups.Add(groupCache);
                            i++; // Pomijamy aktualne pole (nag³ówek)
                            
                            // Dodajemy kolejne pola do grupy a¿ do napotkania StopFold lub kolejnego Binder z foldAll = true.
                            while (i < allProps.Count)
                            {
                                FieldInfo nextFi = GetFieldInfo(allProps[i]);
                                if (nextFi != null)
                                {
                                    if (nextFi.GetCustomAttributes(typeof(StopFold), false).Length > 0)
                                        break;
                                    Binder[] nextBinders = nextFi.GetCustomAttributes(typeof(Binder), false) as Binder[];
                                    if (nextBinders != null && nextBinders.Length > 0 && nextBinders[0].foldAll)
                                        break;
                                }
                                groupCache.groupProps.Add(allProps[i].Copy());
                                i++;
                            }
                            continue;
                        }
                    }
                }
                // Jeœli w³aœciwoœæ nie nale¿y do ¿adnej grupy, dodajemy j¹ do listy niezgrupowanych.
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
            // Binder do nag³ówka grupy (foldAll = true)
            public Binder headerBinder;
            // Opcjonalny Binder do rysowania w³aœciwoœci (gdy mamy dwa atrybuty na jednym polu)
            public Binder fieldBinder;
            // SerializedProperty, która by³a nag³ówkiem (pierwsza z Binderów)
            public SerializedProperty headerProp;
            // Lista w³aœciwoœci nale¿¹cych do grupy
            public List<SerializedProperty> groupProps = new List<SerializedProperty>();
        }
    }
}
