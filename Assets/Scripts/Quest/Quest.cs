using UnityEngine;

namespace QuestSystem
{
    public enum QuestState
    {
        NotStarted,
        Active,
        Completed
    }

    [CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest Data")]
    public class Quest : ScriptableObject
    {
        [Header("Identification")]
        public string questID;

        [Header("Display Information")]
        public string questName;

        [Header("Description")]
        [TextArea]
        public string description;
    }
}