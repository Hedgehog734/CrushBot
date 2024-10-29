using CrushBot.Core.Entities;
using CrushBot.Core.Interfaces.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CrushBot.Infrastructure.Data.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public IQueryable<BotUser> GetUsers()
    {
        return context.Users.AsNoTracking();
    }

    public IQueryable<BotUser> GetUsers(IEnumerable<long> userIds)
    {
        return context.Users
            .Include(x => x.City)
            .Include(x => x.Filter)
            .Include(x => x.Likes)
            .Include(x => x.LikedBy)
            .Where(x => userIds.Contains(x.Id))
            .AsNoTracking()
            .AsSplitQuery();
    }

    public async Task<BotUser?> GetUserAsync(long userId, bool asNoTracking = true)
    {
        var query = context.Users
            .Include(x => x.City)
            .Include(x => x.Filter)
            .Include(x => x.Likes)
            .Include(x => x.LikedBy)
            .AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.AsSplitQuery().SingleOrDefaultAsync(x => x.Id == userId);
    }

    public void AddUser(BotUser user)
    {
        context.Users.Add(user);
    }

    public void UpdateUser(BotUser existing, BotUser updated)
    {
        updated.Id = existing.Id;
        context.Entry(existing).CurrentValues.SetValues(updated);
    }

    public async Task RemoveUserAsync(BotUser user)
    {
        var existing = await context.Users
            .SingleOrDefaultAsync(x => x.Id == user.Id);

        if (existing != null)
        {
            context.Users.Remove(existing);
        }
    }
}