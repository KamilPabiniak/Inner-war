using UnityEngine;

public class QuestInteractable : MonoBehaviour, IInteractable
{
    public string associatedQuestID;

    public void Interact(Player player)
    {
        QuestInstance currentQuest = QuestManager.Instance.GetCurrentQuest();
        if (currentQuest != null && currentQuest.data.questID == associatedQuestID)
        {
            Debug.Log("Ukoñczono quest: " + currentQuest.data.questName);
            QuestManager.Instance.CompleteCurrentQuest();
        }
        else
        {
            Debug.Log("Nie mo¿esz jeszcze wykonaæ tego zadania!");
        }
    }
}