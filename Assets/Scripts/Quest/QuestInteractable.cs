using UnityEngine;

namespace QuestSystem
{
    public class QuestInteractable : MonoBehaviour, IInteractable
    {
        // Optional: used to validate if this interactable is for the current quest
        public string associatedQuestID;

        public void Interact(Player player)
        {
            Quest currentQuest = QuestManager.Instance.GetCurrentQuest();
            if (currentQuest != null && currentQuest.questID == associatedQuestID)
            {
                Debug.Log("Completed quest: " + currentQuest.questName);
                QuestManager.Instance.CompleteCurrentQuest();
            }
        }
    }
}