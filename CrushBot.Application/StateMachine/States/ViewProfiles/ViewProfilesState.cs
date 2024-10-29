using CrushBot.Application.Adapters;
using CrushBot.Application.Interfaces;
using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.Context;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Entities;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using CrushBot.Core.Requests;
using CrushBot.Core.Settings;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.ViewProfiles;

public class ViewProfilesState(
    ITelegramClient client,
    IUserLookupService lookupService,
    IAccountService accountService,
    UserContextProvider provider,
    ILocalizer localizer,
    IAppSettings settings,
    ILogger<ViewProfilesState> logger)
    : BaseWithButtonsState(client, localizer, logger)
{
    private const string UsersBatchKey = nameof(UsersBatchKey);
    private const string CurrentUserIdKey = nameof(CurrentUserIdKey);
    private const string LikerShownKey = nameof(LikerShownKey);
    private const string KeyboardFlagKey = nameof(KeyboardFlagKey);
    private const string ResetMessageKey = nameof(ResetMessageKey);

    private const string Dislike = "❌";
    private const string Message = "💌";
    private const string Like = "💚";

    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message, // todo error handling, like limit
        CancellationToken cancellationToken)
    {
        var language = user.Language;
        var context = provider.GetOrCreateContext(user.Id);

        try
        {
            await EnsureKeyboardAsync(user, message, cancellationToken);

            var isUserSent = false;
            var currentId = context.GetData<long>(CurrentUserIdKey);

            if (currentId != 0)
            {
                var likerShown = context.GetData<bool>(LikerShownKey);
                isUserSent = await SendFeedUser(currentId, likerShown, false);
            }
            else
            {
                int userCount;

                do
                {
                    var likerIds = await GetUserLikerIdsAsync(user);
                    var likerShown = likerIds.Count > 0;
                    context.SetData(LikerShownKey, likerShown);

                    var feedIds = likerShown
                        ? likerIds
                        : await GetFeedUserIdsAsync(user, message, cancellationToken);

                    userCount = feedIds.Count;

                    while (feedIds.Count > 0)
                    {
                        currentId = feedIds.First();
                        isUserSent = await SendFeedUser(currentId, likerShown);

                        feedIds.Remove(currentId);
                    }
                } while (!isUserSent && userCount > 0);
            }

            if (!isUserSent)
            {
                var shareButton = InlineKeyboardButton.WithSwitchInlineQuery("Share", settings.BotLink); // todo localize
                var keyboard = new InlineKeyboardMarkup().AddButton(shareButton);
                var text = Localizer.GetString(language, Messages.NoProfiles);

                await Client.SendMessageAsync(text, message, cancellationToken, keyboard);
            }
        }
        catch
        {
            CleanupContextData(user);
        }

        return;

        async Task<bool> SendFeedUser(long currentId, bool likerShown, bool setCurrentId = true)
        {
            var feedUser = await lookupService.GetUserAsync(currentId);

            if (feedUser != null)
            {
                if (setCurrentId)
                {
                    context.SetData(CurrentUserIdKey, feedUser.Id);
                }

                var status = string.Empty;

                if (likerShown)
                {
                    status = Localizer.GetString(user.Language, Messages.StatusLiked);
                }

                await SendUserAccount(message.Chat.Id, feedUser, language, cancellationToken, status);
                return true;
            }

            return false;
        }
    }

    protected override async Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;
        var text = message.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var backText = GetBackButton(language);

            if (text.Equals(backText, StringComparison.OrdinalIgnoreCase))
            {
                CleanupContextData(user);
                return StateTrigger.PreviousStep;
            }

            var context = provider.GetOrCreateContext(user.Id);
            var feedUser = await GetCurrentFeedUser(context);
            
            if (feedUser == null)
            {
               return StateTrigger.DataProcessed;
            }


            if (text.Equals(Dislike, StringComparison.OrdinalIgnoreCase))
            {
                AddLike(user, feedUser, false);
                context.RemoveData(CurrentUserIdKey);
                return StateTrigger.DataProcessed;
            }

            if (text.Equals(Like, StringComparison.OrdinalIgnoreCase))
            {
                AddLike(user, feedUser, true);

                var feedUserLiked = DoesLikeExist(feedUser, true);

                if (feedUserLiked)
                {
                    await HandleMatch(user, feedUser, message, cancellationToken);
                    context.RemoveData(CurrentUserIdKey);
                    return StateTrigger.DataProcessed;
                }

                var feedUserDisliked = DoesLikeExist(feedUser, false);

                if (feedUserDisliked)
                {
                    context.RemoveData(CurrentUserIdKey);
                    return StateTrigger.DataProcessed;
                }

                var likerShown = context.GetData<bool>(LikerShownKey);

                if (likerShown)
                {
                    await HandleMatch(user, feedUser, message, cancellationToken);
                }
                else
                {
                    await NotifyLikedUser(feedUser, cancellationToken);
                }

                context.RemoveData(CurrentUserIdKey);
                return StateTrigger.DataProcessed;
            }

            // todo message button handle
        }

        return StateTrigger.InvalidData;

        bool DoesLikeExist(BotUserDto feedUser, bool value)
        {
            return feedUser.Likes.Any(x => x.LikedUserId == user.Id && x.Value == value);
        }
    }

    public override UserState State => UserState.ViewProfiles;

    private async Task<List<long>> GetFeedUserIdsAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var context = provider.GetOrCreateContext(user.Id);
        var feedIds = context.GetData<List<long>>(UsersBatchKey) ?? [];

        if (feedIds.Count == 0)
        {
            var request = new SearchRequest(user.ToEntity());
            feedIds = await lookupService.GetFilteredUserIdsAsync(user, request);

            if (feedIds.Count == 0)
            {
                var isReset = lookupService.ResetRequestAge(user, request);

                if (isReset)
                {
                    await EnsureResetMessageAsync(user, message, context, cancellationToken);
                    feedIds = await lookupService.GetFilteredUserIdsAsync(user, request);
                }
            }

            await lookupService.LoadUsersAsync(feedIds);
            context.SetData(UsersBatchKey, feedIds);
        }

        return feedIds;
    }

    private async Task<List<long>> GetUserLikerIdsAsync(BotUserDto user)
    {
        var likerIds = user.LikedBy.Where(x => x.Value).Select(x => x.LikerUserId).ToList();
        var likedIds = user.Likes.Select(x => x.LikedUserId);
        likerIds = likerIds.Except(likedIds).ToList();

        if (likerIds.Count > 0)
        {
            await lookupService.LoadUsersAsync(likerIds);
        }

        return likerIds;
    }

    private async Task EnsureKeyboardAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;
        var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };

        keyboard.AddNewRow(Dislike, Message, Like)
            .AddNewRow(GetBackButton(language));

        keyboard = ReverseKeyboardIfRtl(keyboard, language);

        var context = provider.GetOrCreateContext(user.Id);
        var keyboardShown = context.GetData<bool>(KeyboardFlagKey);

        if (!keyboardShown)
        {
            await Client.SendMessageAsync("🔍", message, cancellationToken, keyboard);
            context.SetData(KeyboardFlagKey, true);
        }
    }

    private async Task EnsureResetMessageAsync(BotUserDto user, Message message,
        UserContext context, CancellationToken cancellationToken)
    {
        var resetMessageShown = context.GetData<bool>(ResetMessageKey);

        if (!resetMessageShown)
        {
            var text = Localizer.GetString(user.Language, Messages.AgeFilterReset);
            await Client.SendMessageAsync(text, message, cancellationToken);
            context.SetData(ResetMessageKey, true);
        }
    }

    private async Task<BotUserDto?> GetCurrentFeedUser(UserContext context)
    {
        var userId = context.GetData<long>(CurrentUserIdKey);

        if (userId == 0)
        {
            return null;
        }

        return await lookupService.GetUserAsync(userId);
    }

    private static void AddLike(BotUserDto liker, BotUserDto liked, bool value)
    {
        var like = new UserLike(liker.Id, liked.Id, value);
        liker.Likes.Add(like);
        liked.LikedBy.Add(like);
    }

    private async Task SendUserAccount(long chatId, BotUserDto user, Language language,
        CancellationToken cancellationToken, string? status = null)
    {
        var account = await accountService.GetProfile(user, language, status);
        await Client.SendMediaGroupAsync(chatId, account, true, cancellationToken);
    }

    private async Task HandleMatch(BotUserDto liker, BotUserDto liked, Message message,
        CancellationToken cancellationToken)
    {
        string status;
        var likedUsername = await Client.GetUserName(liked.Id, cancellationToken);

        // todo sync
        if (!string.IsNullOrWhiteSpace(likedUsername))
        {
            await NotifyOfMatch(liker, liked, likedUsername);
        }

        var likerUsername = message.From?.Username;

        if (!string.IsNullOrWhiteSpace(likerUsername))
        {
            await NotifyOfMatch(liked, liker, likerUsername);
        }

        return;

        async Task NotifyOfMatch(BotUserDto notifiee, BotUserDto userSent, string usernameSent)
        {
            if (notifiee.State == UserState.ViewMatches)
            {
                status = Localizer.GetFormatted(notifiee.Language, Messages.StatusMatched, usernameSent.AddAtSign()); // todo view in match step by button only
                await SendUserAccount(notifiee.Id, userSent, notifiee.Language, cancellationToken, status);
            }
            else if (notifiee.State == UserState.Profile)
            {
                // todo hide\show matches button?
            }
            else
            {
                var text = Localizer.GetFormatted(notifiee.Language, Messages.NewMatch, 1); // todo matches count
                await Client.SendMessageAsync(text, notifiee.Id, cancellationToken, null, ParseMode.MarkdownV2);
            }
        }
    }

    private async Task NotifyLikedUser(BotUserDto user, CancellationToken cancellationToken)
    {
        string actionText;
        var language = user.Language;

        var newLikeText = Localizer.GetFormatted(language, Messages.NewLike, user.LikedBy.Count)
            .ConvertDigits(language);

        if (user.State == UserState.ViewProfiles)
        {
            var currentUserId = provider.GetData<long>(user, CurrentUserIdKey);
            var message = currentUserId == 0 ? Messages.PressAny : Messages.Proceed;
            actionText = Localizer.GetString(language, message);
        }
        else
        {
            actionText = Localizer.GetString(language, Messages.StartSearch);
        }

        var text = $"{newLikeText}\n\n{actionText}";
        await Client.SendMessageAsync(text, user.Id, cancellationToken, null, ParseMode.MarkdownV2);
    }

    private void CleanupContextData(BotUserDto user)
    {
        var context = provider.GetOrCreateContext(user.Id);
        context.RemoveData(UsersBatchKey);
        context.RemoveData(CurrentUserIdKey);
        context.RemoveData(LikerShownKey);
        context.RemoveData(KeyboardFlagKey);
        context.RemoveData(ResetMessageKey);
    }
}