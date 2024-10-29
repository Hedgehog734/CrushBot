using CrushBot.Core.Entities;

namespace CrushBot.Core.Interfaces.Data.Repositories;

public interface IUserRepository
{
    IQueryable<BotUser> GetUsers();

    IQueryable<BotUser> GetUsers(IEnumerable<long> userIds);

    Task<BotUser?> GetUserAsync(long userId, bool asNoTracking = true);

    public void AddUser(BotUser user);

    void UpdateUser(BotUser existing, BotUser updated);

    Task RemoveUserAsync(BotUser user);
}