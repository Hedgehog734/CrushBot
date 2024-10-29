using CrushBot.Application.Models;

namespace CrushBot.Application.StateMachine.Context;

public static class ContextProviderExtensions
{
    public static void SetData(this UserContextProvider provider, BotUserDto user, string key, object data)
    {
        var context = provider.GetOrCreateContext(user.Id);
        context.SetData(key, data);
    }

    public static T? GetData<T>(this UserContextProvider provider, BotUserDto user, string key)
    {
        var context = provider.GetOrCreateContext(user.Id);
        return context.GetData<T>(key);
    }

    public static void RemoveData(this UserContextProvider provider, BotUserDto user, string key)
    {
        var context = provider.GetOrCreateContext(user.Id);
        context.RemoveData(key);
    }
}