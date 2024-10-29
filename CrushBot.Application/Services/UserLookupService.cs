using CrushBot.Application.Interfaces;
using CrushBot.Application.Models;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Requests;
using Microsoft.EntityFrameworkCore;

namespace CrushBot.Application.Services;

public class UserLookupService(
    IUserManager userManager,
    IUserService userService,
    IAgeService ageService)
    : IUserLookupService
{
    public async Task<List<long>> GetFilteredUserIdsAsync(BotUserDto user,
        SearchRequest request, int batchSize = 30)
    {
        var excluded = user.Likes.Select(x => x.LikedUserId).Concat([user.Id]);

        return await userService.GetUserIds(request, excluded)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task<BotUserDto?> GetUserAsync(long userId)
    {
        return await userManager.ReceiveFeedUserAsync(userId);
    }

    public async Task LoadUsersAsync(List<long> userIds)
    {
        var missingIds = userIds
            .Where(x => !userManager.IsCacheUserExist(x))
            .ToList();

        if (missingIds.Count > 0)
        {
            await userManager.LoadFeedDbUsersAsync(missingIds);
        }
    }

    public bool ResetRequestAge(BotUserDto user, SearchRequest request)
    {
        var (min, max) = ageService.GetAllowedRange(user);
        var isUpdated = false;

        if (request.MinAge != min)
        {
            request.MinAge = min;
            isUpdated = true;
        }

        if (request.MaxAge != max)
        {
            request.MaxAge = max;
            isUpdated = true;
        }

        return isUpdated;
    }
}