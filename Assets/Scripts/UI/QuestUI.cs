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
            {
                QuestManager.Instance.OnQuestUpdated += UpdateQuestUI;
                // Initialize quest UI with the current quest
                Quest currentQuest = QuestManager.Instance.GetCurrentQuest();
                if (currentQuest != null)
                    UpdateQuestUI(currentQuest);
            }
        }

        private void UpdateQuestUI(Quest quest)
        {
            if (quest != null)
            {
                questTitleText.text = quest.questName;
                questDescriptionText.text = quest.description;
            }
            else
            {
                questTitleText.text = "No active quest";
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