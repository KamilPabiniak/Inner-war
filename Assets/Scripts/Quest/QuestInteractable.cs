using UnityEngine;

namespace QuestSystem
{
    public class QuestInteractable : MonoBehaviour, IInteractable
    {
        [Tooltip("The Quest ID associated with this interactable object.")]
        public string associatedQuestID;

        public void Interact(Player player)
        {
            QuestInstance currentQuest = QuestManager.Instance.GetCurrentQuest();
            if (currentQuest != null && currentQuest.Data.questID == associatedQuestID)
            {
                Debug.Log("Completed quest: " + currentQuest.Data.questName);
                QuestManager.Instance.CompleteCurrentQuest();
            }
        }
    }
}