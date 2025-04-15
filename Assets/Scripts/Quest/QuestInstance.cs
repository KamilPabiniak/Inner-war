namespace QuestSystem
{
    public class QuestInstance
    {
        public Quest Data { get; private set; }
        public QuestState State { get; set; }

        public QuestInstance(Quest questData)
        {
            Data = questData;
            State = QuestState.NotStarted;
        }
    }
}