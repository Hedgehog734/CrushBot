using CrushBot.Application.Models;

namespace CrushBot.Application.Facades.Interfaces;

public interface IUserDistributedFacade
{
    BotUserDto? GetCacheUser(long userId);

    Task<BotUserDto?> GetDbUserAsync(long userId);

    IEnumerable<BotUserDto> GetDbUsers(IEnumerable<long> userIds);

    Task<bool> UpdateUserAsync(long userId, Action<BotUserDto> action);

    Task UpdateUserAsync(BotUserDto user, bool onlyCache = true);

    Task RemoveUserAsync(BotUserDto user);

    void SetCacheEvictionHandler();
}