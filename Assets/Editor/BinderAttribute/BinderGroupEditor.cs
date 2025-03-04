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
        // List to store all serialized properties in declaration order.
        private readonly List<SerializedProperty> _allProps = new List<SerializedProperty>();

        // Ordered list of items (either ungrouped properties or grouped items) preserving the original order.
        private readonly List<OrderedItem> _orderedItems = new List<OrderedItem>();

        // Cached target type to avoid repeated GetType() calls.
        private System.Type _targetType;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SetupOrderedItems();

            // Draw the m_Script field at the top if present.
            DrawScriptField();

            // Draw all properties and groups in the original order.
            foreach (var item in _orderedItems)
            {
                item.Draw(this);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawScriptField()
        {
            if (_allProps.Count > 0 && _allProps[0].propertyPath == "m_Script")
            {
                EditorGUILayout.PropertyField(_allProps[0], true);
                EditorGUILayout.Space();
            }
        }

        // Build the ordered list of items based on the original declaration order.
        void SetupOrderedItems()
        {
            _orderedItems.Clear();
            _allProps.Clear();

            // Cache target type once.
            _targetType = target.GetType();

            SerializedProperty prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    _allProps.Add(prop.Copy());
                }
                while (prop.NextVisible(false));
            }

            int i = 0;
            while (i < _allProps.Count)
            {
                SerializedProperty currentProp = _allProps[i];

                // Skip the m_Script property.
                if (currentProp.propertyPath == "m_Script")
                {
                    i++;
                    continue;
                }

                FieldInfo fi = GetFieldInfo(currentProp);
                if (fi != null)
                {
                    // If the property has a Binder with foldAll == true, treat it as a group header.
                    if (fi.GetCustomAttributes(typeof(Binder), false) is Binder[] { Length: > 0 } binders && binders[0].foldAll)
                    {
                        FoldGroupCache groupCache = new FoldGroupCache
                        {
                            headerBinder = binders[0]
                        };

                        // If a second Binder is provided, add the property to the group so that it is drawn with its default display name.
                        if (binders.Length > 1)
                        {
                            groupCache.groupProps.Add(currentProp.Copy());
                        }
                        i++; // Skip the header.

                        // Collect subsequent properties belonging to this group until a StopFold or next group header is encountered.
                        while (i < _allProps.Count)
                        {
                            FieldInfo nextFi = GetFieldInfo(_allProps[i]);
                            if (nextFi != null)
                            {
                                if (nextFi.GetCustomAttributes(typeof(StopFold), false).Length > 0)
                                    break;
                                if (nextFi.GetCustomAttributes(typeof(Binder), false) is Binder[] { Length: > 0 } nextBinders && nextBinders[0].foldAll)
                                    break;
                            }
                            groupCache.groupProps.Add(_allProps[i].Copy());
                            i++;
                        }
                        _orderedItems.Add(new GroupItem { group = groupCache });
                        continue;
                    }
                }
                // If the property does not belong to any group, add it as an ungrouped property.
                _orderedItems.Add(new PropertyItem { property = currentProp.Copy() });
                i++;
            }
        }

        // Retrieve FieldInfo for a serialized property by its name using the cached target type.
        FieldInfo GetFieldInfo(SerializedProperty property)
        {
            return _targetType.GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        // Draws a group header using the Binder attribute spacing settings.
        void DrawGroup(FoldGroupCache group)
        {
            // Create a unique key for storing the foldout state.
            string prefsKey = "Binder_" + group.headerBinder.header + "_" + target.GetInstanceID();
            bool isExpanded = EditorPrefs.GetBool(prefsKey, group.headerBinder.foldAll);

            // Calculate the total header height: topSpace + standard line height + bottomSpace.
            float headerHeight = group.headerBinder.topSpace + EditorGUIUtility.singleLineHeight + group.headerBinder.bottomSpace;

            // Reserve a control rect for the header.
            Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight);

            // Calculate the rect for the label after applying the top spacing.
            Rect labelRect = new Rect(headerRect.x, headerRect.y + group.headerBinder.topSpace, headerRect.width, EditorGUIUtility.singleLineHeight);

            // Draw the foldout arrow (without label) in the same rect.
            isExpanded = EditorGUI.Foldout(labelRect, isExpanded, GUIContent.none, true);

            // Retrieve the style from BinderDrawer (includes font, alignment, and color settings).
            GUIStyle style = BinderDrawer.GetStyle(group.headerBinder);

            // Set the content color based on Binder.colorHex.
            Color originalColor = GUI.contentColor;
            if (!ColorUtility.TryParseHtmlString(group.headerBinder.colorHex, out Color headerColor))
                headerColor = originalColor;
            GUI.contentColor = headerColor;

            // Draw the header label using the specified style.
            EditorGUI.LabelField(labelRect, group.headerBinder.header, style);

            // Restore the original content color.
            GUI.contentColor = originalColor;

            // Save the foldout state.
            EditorPrefs.SetBool(prefsKey, isExpanded);

            // If the group is collapsed, exit here.
            if (!isExpanded) return;

            // Increase indentation for grouped properties.
            EditorGUI.indentLevel++;
            foreach (var prop in group.groupProps)
            {
                EditorGUILayout.PropertyField(prop, true);
            }
            EditorGUI.indentLevel--;
        }

        // Abstract base class representing an item in the ordered list.
        abstract class OrderedItem
        {
            public abstract void Draw(BinderGroupEditor editor);
        }

        // Represents an ungrouped property.
        class PropertyItem : OrderedItem
        {
            public SerializedProperty property;
            public override void Draw(BinderGroupEditor editor)
            {
                EditorGUILayout.PropertyField(property, true);
            }
        }

        // Represents a group of properties.
        class GroupItem : OrderedItem
        {
            public FoldGroupCache group;
            public override void Draw(BinderGroupEditor editor)
            {
                editor.DrawGroup(group);
            }
        }

        // Internal cache structure for grouped properties.
        private class FoldGroupCache
        {
            public Binder headerBinder; // Binder used for the group header (foldAll = true)
            public readonly List<SerializedProperty> groupProps = new List<SerializedProperty>(); // List of properties in the group
        }
    }
}
