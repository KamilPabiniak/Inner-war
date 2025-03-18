using UnityEngine;

public enum QuestState
{
    NotStarted,
    Active,
    Completed
}

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest Data")]
public class Quest : ScriptableObject
{
    public string questID;
    public string questName;
    [TextArea]
    public string description;
    
}