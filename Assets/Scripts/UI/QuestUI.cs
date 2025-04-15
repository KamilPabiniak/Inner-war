using TMPro;
using UnityEngine;

namespace QuestSystem
{
    public class QuestUI : MonoBehaviour
    {
        public TextMeshProUGUI questTitleText;
        public TextMeshProUGUI questDescriptionText;

        private void Start()
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.OnQuestUpdated += UpdateQuestUI;

            QuestInstance currentQuest = QuestManager.Instance?.GetCurrentQuest();
            if (currentQuest != null)
                UpdateQuestUI(currentQuest);
        }

        private void UpdateQuestUI(QuestInstance quest)
        {
            if (quest.State == QuestState.Active)
            {
                questTitleText.text = quest.Data.questName;
                questDescriptionText.text = quest.Data.description;
            }
            else if (quest.State == QuestState.Completed)
            {
                questTitleText.text = quest.Data.questName + " - COMPLETED";
                questDescriptionText.text = "";
            }
        }

        private void OnDestroy()
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.OnQuestUpdated -= UpdateQuestUI;
        }
    }
}