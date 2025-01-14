public interface IFearBehavior
{
    void Enter(AnxietyManager manager);
    void UpdateEffects();
    void Exit();
}