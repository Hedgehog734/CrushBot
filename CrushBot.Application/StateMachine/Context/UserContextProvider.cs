using System.Collections.Concurrent;

namespace CrushBot.Application.StateMachine.Context;

public class UserContextProvider
{
    private readonly ConcurrentDictionary<long, UserContext> _userContexts = new();

    public UserContext GetOrCreateContext(long userId)
    {
        return _userContexts.GetOrAdd(userId, new UserContext());
    }

    public void RemoveContext(long userId)
    {
        _userContexts.TryRemove(userId, out _);
    }
}