using UnityEngine;

public class ModerateFearBehavior : IFearBehavior
{
    private AnxietyManager _manager;

    public void Enter(AnxietyManager manager)
    {
        _manager = manager;
        Debug.Log("Entering ModerateFearBehavior.");
    }

    public void UpdateEffects()
    {
        
    }

    public void Exit()
    {
        Debug.Log("Exiting ModerateFearBehavior.");
    }
}