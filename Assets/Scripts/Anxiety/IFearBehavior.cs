public interface IFearBehavior
{
    void Enter(AnxietyManager manager);
    void UpdateEffects();
    void Exit();
}

public interface IFearEffect
{
    void Activate();
    void Deactivate();
}
