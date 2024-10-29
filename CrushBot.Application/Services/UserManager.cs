using CrushBot.Application.Facades.Interfaces;
using CrushBot.Application.Interfaces;
using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.Extensions;
using CrushBot.Core.Interfaces;

namespace CrushBot.Application.Services;

public class UserManager : IUserManager
{
    private readonly ITelegramClient _client;
    private readonly IUserDistributedFacade _userFacade;
    private readonly ISubscriptionService _subService;
    private readonly IWeightService _weightService;

    public UserManager(
        ITelegramClient client,
        IUserDistributedFacade userFacade,
        ISubscriptionService subService,
        IWeightService weightService)
    {
        _client = client;
        _userFacade = userFacade;
        _subService = subService;
        _weightService = weightService;

        userFacade.SetCacheEvictionHandler();
    }

    public async Task<BotUserDto> ReceiveCurrentUserAsync(long userId, bool onlyDb = false)
    {
        BotUserDto? userDto;

        if (!onlyDb)
        {
            userDto = _userFacade.GetCacheUser(userId);

            if (userDto != null)
            {
                await _weightService.UpdateCurrentUserWeight(userDto);
                return userDto;
            }
        }

        userDto = await _userFacade.GetDbUserAsync(userId);
        var isSubscribed = await _subService.IsUserSubscribedAsync(userId, _client);

        if (userDto != null)
        {
            await _weightService.UpdateCurrentUserWeight(userDto);
            userDto.AdjustBasedOnSubscription(_subService, isSubscribed);
            await _userFacade.UpdateUserAsync(userDto);

            return userDto;
        }

        userDto = new BotUserDto
        {
            Id = userId,
            IsSubscribed = isSubscribed,
            ShowEmoji = isSubscribed
        };

        await _weightService.UpdateCurrentUserWeight(userDto);
        await _userFacade.UpdateUserAsync(userDto);
        return userDto;
    }

    public async Task<BotUserDto?> ReceiveFeedUserAsync(long userId)
    {
        var userDto = _userFacade.GetCacheUser(userId);

        if (userDto != null)
        {
            if (userDto.NeedUpdate)
            {
                var isSubscribed = await _subService.IsUserSubscribedAsync(userId, _client);
                userDto.AdjustBasedOnSubscription(_subService, isSubscribed);

                userDto.NeedUpdate = false;
            }

            await _weightService.UpdateFeedUserWeight(userDto);
        }

        return userDto;
    }

    public async Task LoadFeedDbUsersAsync(IEnumerable<long> userIds)
    {
        var usersDto = _userFacade.GetDbUsers(userIds);

        foreach (var userDto in usersDto)
        {
            userDto.NeedUpdate = true;
            await _userFacade.UpdateUserAsync(userDto);
        }
    }

    public async Task UpdateCurrentUserAsync(BotUserDto user, bool onlyCache = true)
    {
        await _weightService.UpdateCurrentUserWeight(user);
        await _userFacade.UpdateUserAsync(user, onlyCache);
    }

    public async Task RemoveUserAsync(BotUserDto user)
    {
        await _userFacade.RemoveUserAsync(user);
    }

    public bool IsCacheUserExist(long userId)
    {
        return _userFacade.GetCacheUser(userId) != null;
    }
}