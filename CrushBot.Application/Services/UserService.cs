using CrushBot.Application.Services.Exceptions;
using CrushBot.Core.Entities;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Interfaces.Data;
using CrushBot.Core.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CrushBot.Application.Services;

public class UserService(IUnitOfWork uow, ILogger<UserService> logger) : IUserService
{
    public IQueryable<long> GetUserIds(SearchRequest request, IEnumerable<long> excludedIds)
    {
        var query = uow.Users.GetUsers()
            .Where(x =>
                x.State >= UserState.Profile &&
                x.CityId == request.CityId &&
                x.Age >= request.MinAge &&
                x.Age <= request.MaxAge &&
                x.Weight != 0 &&
                !excludedIds.Contains(x.Id)); // todo indexes and index for city keys

        if (request.Sex != Sex.Any)
        {
            query = query.Where(x => x.Sex == request.Sex); // todo test
        }

        return query.OrderByDescending(x => x.Weight).Select(x => x.Id);
    }

    public IQueryable<BotUser> GetUsersAsync(IEnumerable<long> userIds)
    {
        return uow.Users.GetUsers(userIds);
    }

    public async Task<BotUser?> GetUserAsync(long userId)
    {
        return await uow.Users.GetUserAsync(userId);
    }

    public async Task UpdateUserAsync(BotUser user)
    {
        try
        {
            if (user.State is not UserState.Registration)
            {
                var existing = await uow.Users.GetUserAsync(user.Id, false);

                if (existing != null)
                {
                    uow.Users.UpdateUser(existing, user);
                    uow.Filters.UpdateFilter(existing.Filter!, user.Filter!);
                    uow.Likes.UpdateUserLikes(existing.Likes, user.Likes);
                }
                else
                {
                    uow.Users.AddUser(user);
                }

                await uow.SaveChangesAsync();
            }
        }
        catch (DbUpdateException ex)
        {
            var message = $"Database update error: {ex.Message}";
            logger.LogWarning(message);
            throw new ServiceException(message, ex);
        }
        catch (Exception ex)
        {
            var message = $"Error while updating user with key {user.Id}. " +
                          $"Id: {user.Id}; Message: {ex.Message}";

            logger.LogError(message);
            throw new ServiceException(message, ex);
        }
    }

    public async Task RemoveUserAsync(BotUser user)
    {
        try
        {
            await uow.Users.RemoveUserAsync(user);
            await uow.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            var message = $"Database update error: {ex.Message}";
            logger.LogWarning(message);
            throw new ServiceException(message, ex);
        }
        catch (Exception ex)
        {
            var message = $"Error while deleting user with key {user.Id}. " +
                          $"Id: {user.Id}; Message: {ex.Message}";

            logger.LogError(message);
            throw new ServiceException(message, ex);
        }
    }
}