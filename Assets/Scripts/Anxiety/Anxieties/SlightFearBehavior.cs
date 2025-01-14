using UnityEngine;

public class SlightFearBehavior : IFearBehavior
{
    public void Enter(AnxietyManager manager)
    {
        Debug.Log("Entering Calm state.");
    }

    public void UpdateEffects()
    {
        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }
}
