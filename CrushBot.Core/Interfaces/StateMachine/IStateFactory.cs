namespace CrushBot.Core.Interfaces.StateMachine;

public interface IStateFactory
{
    T Create<T>() where T : IState;
}