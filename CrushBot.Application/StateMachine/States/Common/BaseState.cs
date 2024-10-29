using System.Net;
using CrushBot.Application.Models;
using CrushBot.Core.Enums;
using CrushBot.Core.Helpers;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Interfaces.StateMachine;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Common;

public abstract class BaseState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<BaseState> logger)
    : IState
{
    protected readonly ITelegramClient Client = client;
    protected readonly ILocalizer Localizer = localizer;
    protected readonly ILogger<BaseState> Logger = logger;

    public async Task<bool> OnEnterAsync<T>(T user, Message message,
        CancellationToken cancellationToken)
    {
        var userDto = user as BotUserDto;

        try
        {
            await OnEnterCoreAsync(userDto!, message, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            await HandleException(ex, userDto!.Language, message, cancellationToken);
            return false;
        }
    }

    public async Task<StateTrigger> HandleAsync<T>(T user, Message message,
        CancellationToken cancellationToken)
    {
        var userDto = user as BotUserDto;

        try
        {
            if (!AllowedTypes.Contains(message.Type))
            {
                return StateTrigger.WrongMessageType;
            }

            return await HandleCoreAsync(userDto!, message, cancellationToken);
        }
        catch (Exception ex)
        {
            await HandleException(ex, userDto!.Language, message, cancellationToken);
            return StateTrigger.UnhandledError;
        }
    }

    public abstract UserState State { get; }

    public virtual bool SaveUserToDatabase => false;

    public virtual bool RefreshFromCache => false;

    public virtual bool RefreshFromDb => false;

    protected abstract Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken);

    protected abstract Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken);

    protected virtual MessageType[] AllowedTypes { get; } = [MessageType.Text];

    protected static ReplyKeyboardMarkup ReverseKeyboardIfRtl(ReplyKeyboardMarkup keyboard, Language language)
    {
        var keyboardRows = keyboard.Keyboard;

        var reversedKeyboard = keyboardRows.Select(row =>
        {
            var rowButtons = row.ToList();

            if (rowButtons.Count > 1 && language is Language.Arabic or Language.Persian)
            {
                return rowButtons.AsEnumerable().Reverse();
            }

            return rowButtons;
        });

        return new ReplyKeyboardMarkup(reversedKeyboard)
        {
            ResizeKeyboard = keyboard.ResizeKeyboard,
            OneTimeKeyboard = keyboard.OneTimeKeyboard,
            IsPersistent = keyboard.IsPersistent,
            InputFieldPlaceholder = keyboard.InputFieldPlaceholder
        };
    }

    private async Task HandleException(Exception exception, Language language, Message message,
        CancellationToken cancellationToken)
    {
        var codes = new[] { HttpStatusCode.Forbidden, HttpStatusCode.TooManyRequests };

        if (ExceptionHelper.IsApiException(exception, out var apiEx, codes))
        {
            throw apiEx;
        }

        if (ExceptionHelper.IsRequestException(exception, out var requestEx, codes))
        {
            throw requestEx;
        }

        Logger.LogError(exception, exception.Message);

        var langCode = message.From!.LanguageCode;
        var text = Localizer.GetString(language, Messages.UnhandledError, langCode);
        await Client.SendMessageAsync(text, message, cancellationToken);
    }
}