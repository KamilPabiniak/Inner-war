#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using QuestSystem; 

public class QuestManagerEditorWindow : EditorWindow
{
    private SerializedObject _serializedQuestManager;
    private SerializedProperty _questsProperty;
    private ReorderableList _questReorderableList;
    private QuestManager _questManager;

    private const string QuestFolder = "Assets/Scenes/Quests";

    private Vector2 _detailsScrollPos;
    private Vector2 _listScrollPos; 

    [MenuItem("Window/Quest Manager Editor")]
    public static void ShowWindow()
    {
        GetWindow<QuestManagerEditorWindow>("Quest Manager Editor");
    }

    [Obsolete("Obsolete")]
    private void OnEnable()
    {
        _questManager = FindObjectOfType<QuestManager>();
        if (_questManager != null)
        {
            _serializedQuestManager = new SerializedObject(_questManager);
            _questsProperty = _serializedQuestManager.FindProperty("quests");
            SetupReorderableList();
        }
    }

    [Obsolete("Obsolete")]
    private void SetupReorderableList()
    {
        _questReorderableList = new ReorderableList(_serializedQuestManager, _questsProperty, true, true, false, false);
        _questReorderableList.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Quest List (Drag to Reorder)");
        };

        _questReorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            SerializedProperty questProp = _questsProperty.GetArrayElementAtIndex(index);
            Quest quest = questProp.objectReferenceValue as Quest;
            rect.y += 2;
            float lineHeight = EditorGUIUtility.singleLineHeight;

            if (quest != null)
            {
                List<string> attachedObjNames = new List<string>();
                QuestInteractable[] interactables = FindObjectsOfType<QuestInteractable>();
                foreach (var qi in interactables)
                {
                    if (qi.associatedQuestID == quest.questID)
                    {
                        attachedObjNames.Add(qi.gameObject.name);
                    }
                }
                
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 80, lineHeight), "ID: " + quest.questID);
                EditorGUI.LabelField(new Rect(rect.x + 85, rect.y, 150, lineHeight), "Name: " + quest.questName);
                
                string attachedText = attachedObjNames.Count > 0 ? "Attached" : "Free";
                EditorGUI.LabelField(new Rect(rect.x + rect.width - 80, rect.y, 80, lineHeight), attachedText);
            }
            else
            {
                EditorGUI.LabelField(rect, "Missing Quest Asset");
            }
        };

        _questReorderableList.onSelectCallback = list =>
        {
            Quest selectedQuest = _questsProperty.GetArrayElementAtIndex(list.index).objectReferenceValue as Quest;
            if (selectedQuest != null)
            {
                EditorGUIUtility.PingObject(selectedQuest);
            }
        };
    }

    [Obsolete("Obsolete")]
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Quest files are located in: " + QuestFolder, EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (_serializedQuestManager == null)
        {
            EditorGUILayout.HelpBox("QuestManager not found in the scene. Please add a QuestManager to your scene.", MessageType.Warning);
            if (GUILayout.Button("Refresh"))
                OnEnable();
            return;
        }

        _serializedQuestManager.Update();
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Add New Quest", GUILayout.Height(30)))
        {
            CreateNewQuest();
        }

        EditorGUILayout.Space();

        float listHeight = Mathf.Max(100, position.height - 350);
        // U¿ywamy scroll view tylko dla scrollowania pionowego (parametr false oznacza brak pionowego scrolla)
        _listScrollPos = EditorGUILayout.BeginScrollView(_listScrollPos, false, false, GUILayout.Height(listHeight));
        _questReorderableList.DoLayoutList();
        EditorGUILayout.EndScrollView();
        
        if (_questReorderableList.index >= 0 && _questReorderableList.index < _questsProperty.arraySize)
        {
            if (GUILayout.Button("Delete Selected Quest", GUILayout.Height(30)))
            {
                DeleteSelectedQuest();
            }
        }

        _serializedQuestManager.ApplyModifiedProperties();
        EditorGUILayout.Space();
        
        if (_questReorderableList.index >= 0 && _questReorderableList.index < _questsProperty.arraySize)
        {
            // U¿ywamy scroll view tylko dla scrollowania pionowego
            _detailsScrollPos = EditorGUILayout.BeginScrollView(_detailsScrollPos, false, false, GUILayout.Height(250));
            Quest selectedQuest = _questsProperty.GetArrayElementAtIndex(_questReorderableList.index).objectReferenceValue as Quest;
            if (selectedQuest != null)
            {
                EditorGUILayout.LabelField("Quest Details", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();

                selectedQuest.questID = EditorGUILayout.TextField("Quest ID", selectedQuest.questID);
                selectedQuest.questName = EditorGUILayout.TextField("Quest Name", selectedQuest.questName);
                EditorGUILayout.LabelField("Description");
                var textAreaStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
                selectedQuest.description = EditorGUILayout.TextArea(selectedQuest.description, textAreaStyle, GUILayout.Height(60));

                // Lista podpiêtych obiektów
                QuestInteractable[] interactables = FindObjectsOfType<QuestInteractable>();
                List<QuestInteractable> attachedQuests = new List<QuestInteractable>();
                foreach (var qi in interactables)
                {
                    if (qi.associatedQuestID == selectedQuest.questID)
                        attachedQuests.Add(qi);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Attached To:", EditorStyles.boldLabel);
                if (attachedQuests.Count > 0)
                {
                    foreach (var qi in attachedQuests)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("- " + qi.gameObject.name);
                        if (GUILayout.Button("Ping", GUILayout.Width(50)))
                        {
                            EditorGUIUtility.PingObject(qi.gameObject);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No QuestInteractable assigned");
                }

                // Zmiany s¹ zapisywane na bie¿¹co (przy oznaczeniu obiektu jako dirty)
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(selectedQuest);
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
    
    [Obsolete("Obsolete")]
    private void CreateNewQuest()
    {
        if (!AssetDatabase.IsValidFolder(QuestFolder))
        {
            string scenesFolder = "Assets/Scenes";
            if (!AssetDatabase.IsValidFolder(scenesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
            AssetDatabase.CreateFolder(scenesFolder, "Quests");
        }
        
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{QuestFolder}/NewQuest.asset");
        
        Quest newQuest = CreateInstance<Quest>();
        newQuest.questID = Guid.NewGuid().ToString("N").Substring(0, 8);
        newQuest.questName = "New Quest";
        newQuest.description = "Quest description goes here";
        
        AssetDatabase.CreateAsset(newQuest, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        _questsProperty.arraySize++;
        SerializedProperty newElement = _questsProperty.GetArrayElementAtIndex(_questsProperty.arraySize - 1);
        newElement.objectReferenceValue = newQuest;
        _serializedQuestManager.ApplyModifiedProperties();
        
        SetupReorderableList();
    }
    
    [Obsolete("Obsolete")]
    private void DeleteSelectedQuest()
    {
        int index = _questReorderableList.index;
        if (index < 0 || index >= _questsProperty.arraySize)
            return;

        Quest selectedQuest = _questsProperty.GetArrayElementAtIndex(index).objectReferenceValue as Quest;
        if (selectedQuest == null)
        {
            EditorUtility.DisplayDialog("Delete Quest", "The selected quest asset is missing.", "OK");
            return;
        }
        
        if (EditorUtility.DisplayDialog("Confirm Delete",
            "Are you sure you want to delete the quest: " + selectedQuest.questName + "?", "Yes", "No"))
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedQuest);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
            
            _questsProperty.DeleteArrayElementAtIndex(index);
            _serializedQuestManager.ApplyModifiedProperties();
            
            SetupReorderableList();
        }
    }
}
#endif
