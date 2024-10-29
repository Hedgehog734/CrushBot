using CrushBot.Core.Entities;
using CrushBot.Core.Requests;

namespace CrushBot.Core.Interfaces;

public interface IUserService
{
    IQueryable<long> GetUserIds(SearchRequest request, IEnumerable<long> excludedIds);

    IQueryable<BotUser> GetUsersAsync(IEnumerable<long> userIds);

    Task<BotUser?> GetUserAsync(long userId);

    Task UpdateUserAsync(BotUser user);

    Task RemoveUserAsync(BotUser user);
}