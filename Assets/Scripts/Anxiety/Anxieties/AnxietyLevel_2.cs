using Anxiety;
using UnityEngine;

public class AnxietyLevel_2 : IFearBehavior
{
    private AnxietyManager _manager;
    
    public void Enter(AnxietyManager manager)
    {
        _manager = manager;
        Debug.Log("Entering ModerateFearBehavior.");
    }

    public void Exit()
    {
        Debug.Log("Exiting ModerateFearBehavior.");
    }

    public void Execute()
    {

    }
}