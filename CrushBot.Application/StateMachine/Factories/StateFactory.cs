using CrushBot.Core.Interfaces.StateMachine;
using Microsoft.Extensions.DependencyInjection;

namespace CrushBot.Application.StateMachine.Factories;

public class StateFactory(IServiceProvider serviceProvider) : IStateFactory
{
    public T Create<T>() where T : IState
    {
        return serviceProvider.GetRequiredService<T>();
    }
}