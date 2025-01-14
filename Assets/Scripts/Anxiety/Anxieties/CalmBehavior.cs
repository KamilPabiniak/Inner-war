using UnityEngine;

public class CalmBehavior : IFearBehavior
{
    public void Enter(AnxietyManager manager)
    {
        Debug.Log("Entering Calm state.");
    }

    public void UpdateEffects()
    {
    }

    public void Exit()
    {
        Debug.Log("Exiting Calm state.");
    }
}