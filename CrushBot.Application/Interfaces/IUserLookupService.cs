using CrushBot.Application.Models;
using CrushBot.Core.Requests;

namespace CrushBot.Application.Interfaces;

public interface IUserLookupService
{
    Task<List<long>> GetFilteredUserIdsAsync(BotUserDto user, SearchRequest request, int batchSize = 30);

    Task<BotUserDto?> GetUserAsync(long userId);

    Task LoadUsersAsync(List<long> userIds);

    bool ResetRequestAge(BotUserDto user, SearchRequest request);
}