using CrushBot.Application.Adapters;
using CrushBot.Application.Facades.Interfaces;
using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.Context;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Interfaces.Cache;

namespace CrushBot.Application.Facades;

public class UserDistributedFacade(
    IUserService userService,
    IUserMemoryCache<BotUserDto> userCache,
    UserContextProvider provider)
    : IUserDistributedFacade
{
    public BotUserDto? GetCacheUser(long userId)
    {
        return userCache.GetUser(userId);
    }

    public async Task<BotUserDto?> GetDbUserAsync(long userId)
    {
        var user = await userService.GetUserAsync(userId);
        return user?.ToDto();
    }

    public IEnumerable<BotUserDto> GetDbUsers(IEnumerable<long> userIds)
    {
        return userService.GetUsersAsync(userIds).Select(x => x.ToDto());
    }

    public async Task<bool> UpdateUserAsync(long userId, Action<BotUserDto> action)
    {
        var userDto = userCache.GetUser(userId);

        if (userDto != null)
        {
            action(userDto);
            userCache.UpdateUser(userId, userDto);
            await UpdateDbUserAsync(userDto);

            return true;
        }

        userDto = await GetDbUserAsync(userId);

        if (userDto != null)
        {
            action(userDto);
            await UpdateDbUserAsync(userDto);

            return true;
        }

        return false;
    }

    public async Task UpdateUserAsync(BotUserDto user, bool onlyCache = true)
    {
        userCache.UpdateUser(user.Id, user);

        if (!onlyCache)
        {
            await userService.UpdateUserAsync(user.ToEntity());
        }
    }

    public async Task RemoveUserAsync(BotUserDto user)
    {
        await userService.RemoveUserAsync(user.ToEntity());
        userCache.RemoveUser(user.Id);
    }

    public void SetCacheEvictionHandler()
    {
        userCache.OnEviction = async (user, invokerService, isManually) =>
        {
            if (!isManually)
            {
                await invokerService.UpdateUserAsync(user.ToEntity());
            }

            provider.RemoveContext(user.Id);
        };
    }

    private async Task UpdateDbUserAsync(BotUserDto user)
    {
        await userService.UpdateUserAsync(user.ToEntity());
    }
}