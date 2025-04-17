using UnityEngine;
using System;

namespace QuestSystem
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }
        
        [Tooltip("List of available quests (set in the Inspector)")]
        public Quest[] quests;
        
        [Tooltip("Index of the currently active quest")]
        public int currentQuestIndex = 0;

        public event Action<Quest> OnQuestUpdated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                ActivateCurrentQuest();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void ActivateCurrentQuest()
        {
            if (currentQuestIndex < quests.Length)
            {
                OnQuestUpdated?.Invoke(quests[currentQuestIndex]);
            }
        }

        public Quest GetCurrentQuest()
        {
            if (currentQuestIndex < quests.Length)
                return quests[currentQuestIndex];
            return null;
        }

        public void CompleteCurrentQuest()
        {
            if (currentQuestIndex < quests.Length)
            {
                Debug.Log("Completed quest: " + quests[currentQuestIndex].questName);
                currentQuestIndex++;
                ActivateCurrentQuest();
            }
        }
    }
}