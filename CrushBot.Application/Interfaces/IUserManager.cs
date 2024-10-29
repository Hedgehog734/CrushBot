using CrushBot.Application.Models;

namespace CrushBot.Application.Interfaces;

public interface IUserManager
{
    Task<BotUserDto> ReceiveCurrentUserAsync(long userId, bool onlyDb = false);

    Task<BotUserDto?> ReceiveFeedUserAsync(long userId);

    Task LoadFeedDbUsersAsync(IEnumerable<long> userIds);

    Task UpdateCurrentUserAsync(BotUserDto user, bool onlyCache = true);

    Task RemoveUserAsync(BotUserDto user);

    bool IsCacheUserExist(long userId);
}