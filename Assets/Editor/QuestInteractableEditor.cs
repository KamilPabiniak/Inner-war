#if UNITY_EDITOR
using UnityEditor;
using QuestSystem;
using System.Collections.Generic;

[CustomEditor(typeof(QuestInteractable))]
public class QuestInteractableEditor : Editor
{
    private QuestInteractable _questInteractable;
    private readonly List<Quest> _availableQuests = new();
    private string[] _questNames;

    private void OnEnable()
    {
        _questInteractable = (QuestInteractable)target;
        string[] guids = AssetDatabase.FindAssets("t:Quest");
        _availableQuests.Clear();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Quest questAsset = AssetDatabase.LoadAssetAtPath<Quest>(path);
            if (questAsset != null)
            {
                _availableQuests.Add(questAsset);
            }
        }
        _questNames = new string[_availableQuests.Count];
        for (int i = 0; i < _availableQuests.Count; i++)
        {
            _questNames[i] = _availableQuests[i].questName;
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspectorWithoutAssociatedQuestID();
        
        if (_availableQuests.Count > 0)
        {
            int selectedIndex = 0;
            for (int i = 0; i < _availableQuests.Count; i++)
            {
                if (_availableQuests[i].questID == _questInteractable.associatedQuestID)
                {
                    selectedIndex = i;
                    break;
                }
            }

            int newSelectedIndex = EditorGUILayout.Popup("Associated Quest", selectedIndex, _questNames);
            if (newSelectedIndex != selectedIndex)
            {
                _questInteractable.associatedQuestID = _availableQuests[newSelectedIndex].questID;
                EditorUtility.SetDirty(_questInteractable);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No available quests found in the project.", MessageType.Info);
        }
    }

    private void DrawDefaultInspectorWithoutAssociatedQuestID()
    {
        serializedObject.Update();
        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;
        while (property.NextVisible(enterChildren))
        {
            if (property.name == "associatedQuestID")
                continue;
            EditorGUILayout.PropertyField(property, true);
            enterChildren = false;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
