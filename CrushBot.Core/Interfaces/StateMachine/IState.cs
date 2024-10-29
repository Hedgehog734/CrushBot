using CrushBot.Core.Enums;
using Telegram.Bot.Types;

namespace CrushBot.Core.Interfaces.StateMachine;

public interface IState
{
    Task<bool> OnEnterAsync<T>(T user, Message message, CancellationToken cancellationToken);

    Task<StateTrigger> HandleAsync<T>(T user, Message message, CancellationToken cancellationToken);

    UserState State { get; }

    bool SaveUserToDatabase { get; }

    bool RefreshFromCache { get; }
}