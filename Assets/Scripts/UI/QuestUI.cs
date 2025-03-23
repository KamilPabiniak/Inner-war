using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescriptionText;

    private void Start()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated += UpdateQuestUI;

        QuestInstance currentQuest = QuestManager.Instance.GetCurrentQuest();
        if (currentQuest != null)
            UpdateQuestUI(currentQuest);
    }

    private void UpdateQuestUI(QuestInstance quest)
    {
        if (quest.state == QuestState.Active)
        {
            questTitleText.text = quest.data.questName;
            questDescriptionText.text = quest.data.description;
        }
        else if (quest.state == QuestState.Completed)
        {
            questTitleText.text = quest.data.questName + " - COMPLETED";
            questDescriptionText.text = "";
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated -= UpdateQuestUI;
    }
}