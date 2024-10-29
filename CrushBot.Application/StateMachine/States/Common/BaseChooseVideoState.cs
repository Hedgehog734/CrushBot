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

public abstract class BaseChooseVideoState(
    UserContextProvider provider,
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<BaseChooseVideoState> logger)
    : BaseWithButtonsState(client, localizer, logger)
{
    public const int MaxDurationNoSub = 30;
    private const int MaxDurationSub = 60;
    private const int MinDuration = 5;

    private const int MinResolution = 320;

    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;

        var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };
        keyboard.AddNewRow(GetBackButton(language));

        var allowedDuration = GetAllowedVideoDuration(user);
        var text = Localizer.GetFormattedWithDigits(language, Messages.ChooseVideo, MinDuration, allowedDuration);
        await Client.SendMessageAsync(text, message, cancellationToken, keyboard, ParseMode.MarkdownV2);
    }

    protected override async Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        if (message.Type == MessageType.Text)
        {
            return HandleText(user, message);
        }

        if (message.Type == MessageType.Video)
        {
            return await HandleVideoAsync(user, message, cancellationToken);
        }

        return StateTrigger.WrongMessageType;
    }

    private StateTrigger HandleText(BotUserDto user, Message message)
    {
        var text = message.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var backText = GetBackButton(user.Language);

            if (text.Equals(backText, StringComparison.OrdinalIgnoreCase))
            {
                return StateTrigger.PreviousStep;
            }
        }

        return StateTrigger.InvalidData;
    }

    private async Task<StateTrigger> HandleVideoAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var video = message.Video;

        if (video != null)
        {
            if (video.Width < MinResolution || video.Height < MinResolution)
            {
                return await InvalidResolutionAsync(user.Language, message, cancellationToken);
            }

            var allowedDuration = GetAllowedVideoDuration(user);

            if (video.Duration < MinDuration || video.Duration > allowedDuration)
            {
                return await InvalidDurationAsync(allowedDuration, user.Language, message, cancellationToken);
            }

            user.VideoId = video.FileId;
            ClearBaseMediaData(user);
            return StateTrigger.DataEntered;
        }

        return StateTrigger.WrongMessageType;
    }

    protected override MessageType[] AllowedTypes { get; } = [MessageType.Text, MessageType.Video];

    private async Task<StateTrigger> InvalidResolutionAsync(Language language, Message message,
        CancellationToken cancellationToken)
    {
        var text = Localizer.GetFormattedWithDigits(language, Messages.InvalidResolution, MinResolution);
        await Client.ReplyMessageAsync(text, message, cancellationToken);
        return StateTrigger.InvalidData;
    }

    private async Task<StateTrigger> InvalidDurationAsync(int maxDuration, Language language, Message message,
        CancellationToken cancellationToken)
    {
        var text = Localizer.GetFormattedWithDigits(language, Messages.InvalidDuration, MinDuration, maxDuration);
        await Client.ReplyMessageAsync(text, message, cancellationToken);
        return StateTrigger.InvalidData;
    }

    private void ClearBaseMediaData(BotUserDto user)
    {
        var context = provider.GetOrCreateContext(user.Id);
        context.RemoveData(BaseMediaState.ContextPhotoKey);
        context.RemoveData(BaseMediaState.ContextVideoKey);
    }

    private static int GetAllowedVideoDuration(BotUserDto user)
    {
        return user.IsSubscribed ? MaxDurationSub : MaxDurationNoSub;
    }
}