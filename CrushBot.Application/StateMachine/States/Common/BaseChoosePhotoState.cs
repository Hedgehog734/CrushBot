using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.Context;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Common;

public abstract class BaseChoosePhotoState(
    UserContextProvider provider,
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<BaseChoosePhotoState> logger)
    : BaseWithButtonsState(client, localizer, logger)
{
    public const int MaxPhotoCountNoSub = 3;
    private const int MaxPhotoCountSub = 5;

    private const int MinResolution = 512;

    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;

        var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };
        keyboard.AddNewRow(GetBackButton(language));

        var photoCount = user.PhotoIds.Count;

        if (photoCount > 0)
        {
            keyboard.AddButton(GetContinueButton(language));
        }

        keyboard = ReverseKeyboardIfRtl(keyboard, user.Language);

        var allowedCount = GetAllowedPhotoCount(user);

        var text = photoCount == 0
            ? Localizer.GetFormattedWithDigits(language, Messages.ChoosePhoto, allowedCount)
            : Localizer.GetString(language, Messages.AskAddPhoto);

        await Client.SendMessageAsync(text, message, cancellationToken, keyboard, ParseMode.MarkdownV2);
    }

    protected override async Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        if (message.Type == MessageType.Text)
        {
            return HandleText(user, message);
        }

        if (message.Type == MessageType.Photo)
        {
           return await HandlePhotoAsync(user, message, cancellationToken);
        }

        return StateTrigger.WrongMessageType;
    }

    protected override MessageType[] AllowedTypes { get; } = [MessageType.Text, MessageType.Photo];

    private StateTrigger HandleText(BotUserDto user, Message message)
    {
        var language = user.Language;
        var text = message.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var continueText = GetContinueButton(language);

            if (text.Equals(continueText, StringComparison.OrdinalIgnoreCase)
                && user.PhotoIds.Count > 0)
            {
                return DataEnteredResult(user);
            }

            var backText = GetBackButton(language);

            if (text.Equals(backText, StringComparison.OrdinalIgnoreCase))
            {
                return StateTrigger.PreviousStep;
            }
        }

        return StateTrigger.InvalidData;
    }

    private async Task<StateTrigger> HandlePhotoAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var photo = message.Photo?.LastOrDefault();

        if (photo != null)
        {
            var allowedCount = GetAllowedPhotoCount(user);

            if (user.PhotoIds.Count == allowedCount)
            {
                return DataEnteredResult(user);
            }

            var language = user.Language;

            if (photo.Width < MinResolution || photo.Height < MinResolution)
            {
               return await InvalidResolutionAsync(language, message, cancellationToken);
            }

            user.PhotoIds.Add(photo.FileId);

            var text = Localizer.GetFormattedWithDigits(language, Messages.PhotoAdded,
                user.PhotoIds.Count, allowedCount);

            await Client.ReplyMessageAsync(text, message, cancellationToken);

            return user.PhotoIds.Count == allowedCount
                ? DataEnteredResult(user)
                : StateTrigger.DataProcessed;
        }

        return StateTrigger.WrongMessageType;
    }

    private StateTrigger DataEnteredResult(BotUserDto user)
    {
        ClearBaseMediaData(user);
        return StateTrigger.DataEntered;
    }

    private async Task<StateTrigger> InvalidResolutionAsync(Language language, Message message,
        CancellationToken cancellationToken)
    {
        var text = Localizer.GetFormattedWithDigits(language, Messages.InvalidResolution, MinResolution);
        await Client.ReplyMessageAsync(text, message, cancellationToken);
        return StateTrigger.InvalidData;
    }

    private void ClearBaseMediaData(BotUserDto user)
    {
        var context = provider.GetOrCreateContext(user.Id);
        context.RemoveData(BaseMediaState.ContextPhotoKey);
        context.RemoveData(BaseMediaState.ContextVideoKey);
    }

    private static int GetAllowedPhotoCount(BotUserDto user)
    {
        return user.IsSubscribed ? MaxPhotoCountSub : MaxPhotoCountNoSub;
    }
}