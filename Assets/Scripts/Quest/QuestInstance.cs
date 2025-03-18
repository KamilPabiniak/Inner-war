public class QuestInstance
{
    public Quest data;
    public QuestState state;

    public QuestInstance(Quest questData)
    {
        data = questData;
        state = QuestState.NotStarted;
    }
}