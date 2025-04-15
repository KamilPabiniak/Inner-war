using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }
        [HideInInspector] public List<Quest> quests = new();

        private readonly List<QuestInstance> _questInstances = new();

        [Tooltip("Index of the currently active quest.")]
        public int currentQuestIndex = 0;

        public event Action<QuestInstance> OnQuestUpdated;

        private void Awake()
        {
            // Ensure singleton instance
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeQuests();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeQuests()
        {
            _questInstances.Clear();
            foreach (Quest questData in quests)
            {
                if (questData != null)
                    _questInstances.Add(new QuestInstance(questData));
            }

            if (_questInstances.Count > 0)
            {
                _questInstances[0].State = QuestState.Active;
                NotifyQuestUpdated(_questInstances[0]);
            }
        }

        public QuestInstance GetCurrentQuest()
        {
            if (currentQuestIndex < _questInstances.Count)
                return _questInstances[currentQuestIndex];
            return null;
        }

        public void CompleteCurrentQuest()
        {
            if (currentQuestIndex < _questInstances.Count)
            {
                _questInstances[currentQuestIndex].State = QuestState.Completed;
                NotifyQuestUpdated(_questInstances[currentQuestIndex]);
                currentQuestIndex++;

                if (currentQuestIndex < _questInstances.Count)
                {
                    _questInstances[currentQuestIndex].State = QuestState.Active;
                    NotifyQuestUpdated(_questInstances[currentQuestIndex]);
                }
            }
        }
        
        private void NotifyQuestUpdated(QuestInstance quest)
        {
            OnQuestUpdated?.Invoke(quest);
        }

        public List<QuestInstance> GetAllQuestInstances()
        {
            return _questInstances;
        }
    }
}
