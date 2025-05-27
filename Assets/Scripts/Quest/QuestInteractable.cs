using UnityEngine;

namespace QuestSystem
{
    public class QuestInteractable : MonoBehaviour, IInteractable
    {
        [Header("Quest Association")]
        [Tooltip("Optional: used to validate if this interactable is for the current quest")]
        public string associatedQuestID;

        [SerializeField] private bool destroyAfterInteraction;

        [Header("Monologue (Audio)")]
        [Tooltip("Optional audio clip to play on the player when this quest is completed")]
        [SerializeField] private AudioClip completionClip;
        [SerializeField, Range(0f, 1f)] private float completionVolume = 1f;
        [SerializeField] private float completionDelay = 0f;

        [Header("Anxiety Trigger (Active Timed)")]
        [Tooltip("If enabled, triggers an active anxiety level on quest completion")]
        [SerializeField] private bool triggerAnxiety = false;
        [SerializeField, Range(5, 7), Tooltip("Anxiety level (5-7) to activate")]
        private int anxietyLevel = 5;
        [SerializeField, Tooltip("Duration in seconds for the active anxiety level")]
        private float anxietyDuration = 10f;

        public void Interact(Player player)
        {
            Quest currentQuest = QuestManager.Instance.GetCurrentQuest();
            if (currentQuest != null && currentQuest.questID == associatedQuestID)
            {
                Debug.Log("Completed quest: " + currentQuest.questName);

                // Complete the quest
                QuestManager.Instance.CompleteCurrentQuest();

                // Play optional monologue on player
                if (completionClip != null && player.TryGetComponent<PlayerMonologue>(out var monologue))
                {
                    monologue.PlayMonologue(completionClip, completionVolume, completionDelay);
                }

                // Trigger optional active timed anxiety
                if (triggerAnxiety)
                {
                    try
                    {
                        Anxiety.AnxietyManager.Instance.TriggerActiveTimed(anxietyLevel, anxietyDuration);
                    }
                    catch (System.ArgumentOutOfRangeException e)
                    {
                        Debug.LogWarning($"Anxiety level must be between 5 and 7. Provided: {anxietyLevel}");
                    }
                }
            }

            if (!destroyAfterInteraction) return;
                Destroy(gameObject);
        }
    }
}
