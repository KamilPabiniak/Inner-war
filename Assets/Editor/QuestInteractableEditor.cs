#if UNITY_EDITOR
using UnityEditor;
using QuestSystem;
using System.Collections.Generic;

[CustomEditor(typeof(QuestInteractable))]
public class QuestInteractableEditor : Editor
{
    private QuestInteractable _questInteractable;
    private readonly List<Quest> _availableQuests = new List<Quest>();
    private string[] _questNames;

    private void OnEnable()
    {
        _questInteractable = (QuestInteractable)target;
        RefreshAvailableQuests();
    }

    private void RefreshAvailableQuests()
    {
        _availableQuests.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Quest");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Quest questAsset = AssetDatabase.LoadAssetAtPath<Quest>(path);
            if (questAsset != null)
                _availableQuests.Add(questAsset);
        }
        _questNames = _availableQuests.ConvertAll(q => q.questName).ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw all properties except associatedQuestID
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (prop.name == "associatedQuestID")
                continue;

            EditorGUILayout.PropertyField(prop, true);
        }

        // Quest selection popup
        if (_availableQuests.Count > 0)
        {
            int selectedIndex = _availableQuests.FindIndex(q => q.questID == _questInteractable.associatedQuestID);
            if (selectedIndex < 0) selectedIndex = 0;

            int newSelectedIndex = EditorGUILayout.Popup("Associated Quest", selectedIndex, _questNames);
            if (newSelectedIndex != selectedIndex)
            {
                Undo.RecordObject(_questInteractable, "Change Associated Quest");
                _questInteractable.associatedQuestID = _availableQuests[newSelectedIndex].questID;
                EditorUtility.SetDirty(_questInteractable);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No available quests found in the project.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
