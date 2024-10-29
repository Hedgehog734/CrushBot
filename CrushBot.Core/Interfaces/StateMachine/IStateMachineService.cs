using Telegram.Bot.Types;

namespace CrushBot.Core.Interfaces.StateMachine;

public interface IStateMachineService
{
    Task ProcessMessageAsync(Message message, CancellationToken cancellationToken);
}