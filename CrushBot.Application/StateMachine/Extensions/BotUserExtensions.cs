using CrushBot.Application.Adapters;
using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.States.EditFilters;
using CrushBot.Application.StateMachine.States.EditProfile;
using CrushBot.Application.StateMachine.States.Profile;
using CrushBot.Application.StateMachine.States.Registration;
using CrushBot.Application.StateMachine.States.Settings;
using CrushBot.Application.StateMachine.States.ViewProfiles;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Interfaces.StateMachine;
using static CrushBot.Application.Services.WeightService;

namespace CrushBot.Application.StateMachine.Extensions;

public static class BotUserExtensions
{
    public static IState GetState(this BotUserDto user, IStateFactory factory)
    {
        return user.State switch
        {
            UserState.Registration => factory.Create<RegistrationState>(),
            UserState.AskLanguage => factory.Create<AskLanguageState>(),
            UserState.AskName => factory.Create<AskNameState>(),
            UserState.AskAge => factory.Create<AskAgeState>(),
            UserState.AskSex => factory.Create<AskSexState>(),
            UserState.AskSexFilter => factory.Create<AskSexFilterState>(),
            UserState.AskCity => factory.Create<AskCityState>(),
            UserState.ChooseCity => factory.Create<ChooseCityState>(),
            UserState.AskMedia => factory.Create<AskMediaState>(),
            UserState.ChoosePhoto => factory.Create<ChoosePhotoState>(),
            UserState.ChooseVideo => factory.Create<ChooseVideoState>(),
            UserState.AskDescription => factory.Create<AskDescriptionState>(),
            UserState.Profile => factory.Create<ProfileState>(),
            UserState.ViewProfiles => factory.Create<ViewProfilesState>(),
            UserState.EditProfile => factory.Create<EditProfileState>(),
            UserState.ChangeCity => factory.Create<ChangeCityState>(),
            UserState.ChangeChooseCity => factory.Create<ChangeChooseCityState>(),
            UserState.ChangeMedia => factory.Create<ChangeMediaState>(),
            UserState.ChangeChoosePhoto => factory.Create<ChangeChoosePhotoState>(),
            UserState.ChangeChooseVideo => factory.Create<ChangeChooseVideoState>(),
            UserState.ChangeDescription => factory.Create<ChangeDescriptionState>(),
            UserState.EditFilters => factory.Create<EditFiltersState>(),
            UserState.ChangeSexFilter => factory.Create<ChangeSexFilterState>(),
            UserState.ChangeAgeFilter => factory.Create<ChangeAgeFilterState>(),
            UserState.Settings => factory.Create<SettingsState>(),
            UserState.Subscription => factory.Create<SubscriptionState>(),
            UserState.ChangeLanguage => factory.Create<ChangeLanguageState>(),
            UserState.DeleteProfile => factory.Create<DeleteProfileState>(),
            _ => throw new ArgumentOutOfRangeException(nameof(user.State), user.State, null)
        };
    }

    public static List<long> GetUserMatchedIds(this BotUserDto user)
    {
        var matched = from like in user.Likes
            where like.LikerUserId == user.Id && like.Value && !like.MatchShown
            join likeBy in user.LikedBy
                on new { Liker = like.LikedUserId, Liked = user.Id }
                equals new { Liker = likeBy.LikerUserId, Liked = likeBy.LikedUserId }
            where likeBy.Value
            select like;

        return matched.Select(x => x.LikedUserId).ToList();
    }

    public static BotUserDto AdjustBasedOnSubscription(this BotUserDto user, ISubscriptionService service,
        bool isSubscribed)
    {
        var subChanged = user.IsSubscribed != isSubscribed;
        user.IsSubscribed = isSubscribed;

        if (subChanged)
        {
            if (!isSubscribed)
            {
                var userEntity = service.CutDataBySubscription(user.ToEntity());
                user = userEntity.ToDto();
            }
            else
            {
                user.ShowEmoji = true;
            }
        }

        return user;
    }

    public static BotUserDto UpdateCurrentUserWeight(this BotUserDto user)
    {
        if (user.State >= UserState.Profile)
        {
            var now = DateTime.UtcNow;
            user.UpdateDaysVisited(now).UpdateWeight(now);
        }

        return user;
    }

    public static BotUserDto UpdateFeedUserWeight(this BotUserDto user)
    {
        if (user.State >= UserState.Profile)
        {
            var now = DateTime.UtcNow;
            user.UpdateWeight(now);
        }

        return user;
    }

    private static BotUserDto UpdateDaysVisited(this BotUserDto user, DateTime now)
    {
        var hoursSinceUpdate = (now - user.UpdateTimestamp).TotalHours;

        if (hoursSinceUpdate <= 24)
        {
            return user;
        }

        if (hoursSinceUpdate <= 48)
        {
            user.DaysVisited++;
        }
        else
        {
            user.DaysVisited = 1;
        }

        user.UpdateTimestamp = now;
        return user;
    }
    
    private static BotUserDto UpdateWeight(this BotUserDto user, DateTime now)
    {
        int subOrConsecutiveWeight;

        if (user.IsSubscribed)
        {
            subOrConsecutiveWeight = SubscriptionWeight;
        }
        else
        {
            var consecutiveDays = Math.Min(user.DaysVisited, ConsecutiveDaysStreak);
            subOrConsecutiveWeight = consecutiveDays * ConsecutiveDayWeight;
        }

        var daysSinceLastVisit = (now - user.UpdateTimestamp).Days;
        var lastVisitWeight = Math.Max(0, (LastDaysStreak - daysSinceLastVisit) * LastDayWeight);

        user.Weight = subOrConsecutiveWeight + lastVisitWeight;
        return user;
    }
}