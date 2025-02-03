using Anxiety;

public interface IFearBehavior
{
    void Enter(AnxietyManager manager);
    void Exit();

    void Execute();
}
