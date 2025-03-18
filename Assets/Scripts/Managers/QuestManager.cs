using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    
    public List<Quest> quests = new List<Quest>();
    public List<QuestInstance> questInstances = new List<QuestInstance>();
    public int currentQuestIndex = 0;
    
    public event Action<QuestInstance> OnQuestUpdated;

    private void Awake()
    {
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


    void InitializeQuests()
    {
        questInstances.Clear();
        foreach (var questData in quests)
        {
            questInstances.Add(new QuestInstance(questData));
        }

        if (questInstances.Count > 0)
        {
            questInstances[0].state = QuestState.Active;
            NotifyQuestUpdated(questInstances[0]);
        }
    }
    
    public QuestInstance GetCurrentQuest()
    {
        if (currentQuestIndex < questInstances.Count)
            return questInstances[currentQuestIndex];
        return null;
    }
    
    public void CompleteCurrentQuest()
    {
        if (currentQuestIndex < questInstances.Count)
        {
            questInstances[currentQuestIndex].state = QuestState.Completed;
            NotifyQuestUpdated(questInstances[currentQuestIndex]);
            currentQuestIndex++;

            if (currentQuestIndex < questInstances.Count)
            {
                questInstances[currentQuestIndex].state = QuestState.Active;
                NotifyQuestUpdated(questInstances[currentQuestIndex]);
            }
        }
    }
    
    void NotifyQuestUpdated(QuestInstance quest)
    {
        OnQuestUpdated?.Invoke(quest);
    }
    
    public List<QuestInstance> GetAllQuestInstances()
    {
        return questInstances;
    }
}
