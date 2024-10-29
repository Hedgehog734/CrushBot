using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.Context;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Common;

public abstract class BaseMediaState(
    UserContextProvider provider,
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<BaseMediaState> logger)
    : BaseWithButtonsState(client, localizer, logger)
{
    public const string ContextPhotoKey = nameof(BotUserDto.PhotoIds);
    public const string ContextVideoKey = nameof(BotUserDto.VideoId);

    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        RestoreMedia(user);
        var language = user.Language;

        var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };
        keyboard.AddNewRow(GetPhotoButton(language), GetVideoButton(language));

        if (IsMediaSet(user))
        {
            keyboard.AddNewRow(GetCurrentButton(language));
        }

        keyboard = ReverseKeyboardIfRtl(keyboard, language);

        var text = Localizer.GetString(language, Messages.AskMedia);
        await Client.SendMessageAsync(text, message, cancellationToken, keyboard);
    }

    protected override Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var text = message.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var language = user.Language;
            var currentText = GetCurrentButton(language);

            if (text.Equals(currentText, StringComparison.OrdinalIgnoreCase) &&
                IsMediaSet(user))
            {
                return Task.FromResult(StateTrigger.DataEntered);
            }

            var photoText = GetPhotoButton(language);

            if (text.Equals(photoText, StringComparison.OrdinalIgnoreCase))
            {
                ClearMedia(user);
                return Task.FromResult(StateTrigger.OptionOne);
            }

            var videoText = GetVideoButton(language);

            if (text.Equals(videoText, StringComparison.OrdinalIgnoreCase))
            {
                ClearMedia(user);
                return Task.FromResult(StateTrigger.OptionTwo);
            }
        }

        return Task.FromResult(StateTrigger.InvalidData);
    }

    private string GetPhotoButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.Photo);
    }

    private string GetVideoButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.Video);
    }

    private static void ClearMedia(BotUserDto user)
    {
        user.PhotoIds.Clear();
        user.VideoId = null;
    }

    private void RestoreMedia(BotUserDto user)
    {
        var (photoIds, videoId) = GetContextData(user);

        if (photoIds == null && videoId == null)
        {
            SetContextData(user);
            (photoIds, videoId) = GetContextData(user);
        }

        user.PhotoIds = [..photoIds ?? []];
        user.VideoId = videoId;
    }

    private void SetContextData(BotUserDto user)
    {
        var context = provider.GetOrCreateContext(user.Id);
        context.SetData(ContextPhotoKey, user.PhotoIds);
        context.SetData(ContextVideoKey, user.VideoId);
    }

    private (List<string>? photoIds, string? videoId) GetContextData(BotUserDto user)
    {
        var context = provider.GetOrCreateContext(user.Id);
        var photoIds = context.GetData<List<string>>(ContextPhotoKey);
        var videoId = context.GetData<string?>(ContextVideoKey);
        return (photoIds, videoId);
    }

    private static bool IsMediaSet(BotUserDto user)
    {
        return user is { PhotoIds.Count: > 0 } or { VideoId: not null };
    }
}