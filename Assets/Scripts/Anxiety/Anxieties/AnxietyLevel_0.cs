using Anxiety;
using UnityEngine;

public class AnxietyLevel_0 : IFearBehavior
{
    public void Enter(AnxietyManager manager)
    {
        Debug.Log("Entering Level 0 Anxiety Behavior");
    }

    public void Exit()
    {
        Debug.Log("Exiting Level 0 Anxiety Behavior");
    }

    public void Execute()
    {
        Debug.LogWarning("Execute Level 0 Anxiety Behavior");
    }
}
