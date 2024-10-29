using CrushBot.Application.Adapters;
using CrushBot.Application.Models;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Common;

public abstract class BaseLanguageState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<BaseLanguageState> logger)
    : BaseState(client, localizer, logger)
{
    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };

        keyboard
            .AddNewRow(
                LanguageHelper.GetDisplayName(Language.English),
                LanguageHelper.GetDisplayName(Language.Spanish),
                LanguageHelper.GetDisplayName(Language.PortugueseBrazil)
            )
            .AddNewRow(
                LanguageHelper.GetDisplayName(Language.Ukrainian),
                LanguageHelper.GetDisplayName(Language.Russian),
                LanguageHelper.GetDisplayName(Language.Kazakh)
            )
            .AddNewRow(
                LanguageHelper.GetDisplayName(Language.Uzbek),
                LanguageHelper.GetDisplayName(Language.Turkish),
                LanguageHelper.GetDisplayName(Language.Persian)
            )
            .AddNewRow(
                LanguageHelper.GetDisplayName(Language.Arabic),
                LanguageHelper.GetDisplayName(Language.Hindi),
                LanguageHelper.GetDisplayName(Language.Filipino)
            )
            .AddNewRow(
                LanguageHelper.GetDisplayName(Language.Vietnamese),
                LanguageHelper.GetDisplayName(Language.Indonesian)
            );

        var langCode = message.From!.LanguageCode;
        var language = LanguageHelper.ResolveLanguage(user.ToEntity(), langCode);
        keyboard = ReverseKeyboardIfRtl(keyboard, language);

        var text = Localizer.GetString(language, Messages.AskLanguage);
        await Client.SendMessageAsync(text, message, cancellationToken, keyboard);
    }

    protected override Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var text = message.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var selectedLanguage = LanguageHelper.FromDisplayName(text);

            if (selectedLanguage != Language.None)
            {
                user.Language = selectedLanguage;
                return Task.FromResult(StateTrigger.DataEntered);
            }
        }

        return Task.FromResult(StateTrigger.InvalidData);
    }
}