using System.Text.RegularExpressions;
using CrushBot.Application.Models;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Common;

public abstract class BaseDescriptionState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<BaseDescriptionState> logger)
    : BaseWithButtonsState(client, localizer, logger)
{
    public const int LenMaxNoSub = 140;
    private const int LenMaxSub = 280;

    private const int LenMin = 10;

    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;
        var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };

        keyboard.AddNewRow(GetContinueButton(language));

        if (!string.IsNullOrWhiteSpace(user.Description))
        {
            keyboard.AddNewRow(GetCurrentButton(language));
        }

        var text = Localizer.GetString(language, Messages.AskDescription);
        await Client.SendMessageAsync(text, message, cancellationToken, keyboard, ParseMode.MarkdownV2);
    }

    protected override async Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;
        var description = message.Text?.Trim();

        var allowedLength = user.IsSubscribed ? LenMaxSub : LenMaxNoSub;

        if (!string.IsNullOrWhiteSpace(description))
        {
            var currentText = GetCurrentButton(language);

            if (description.Equals(currentText, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(user.Description))
            {
                return StateTrigger.DataEntered;
            }

            var continueText = GetContinueButton(language);

            if (description.Equals(continueText, StringComparison.OrdinalIgnoreCase))
            {
                user.Description = null;
                return StateTrigger.NextStep;
            }

            var trimmed = TrimWhitespaces(description);

            if (trimmed.Length >= LenMin && trimmed.Length <= allowedLength)
            {
                description = message.ToMarkdown();
                description = TrimWhitespaces(description!);

                user.Description = description;
                return StateTrigger.DataEntered;
            }
        }

        var invalidLengthText = Localizer.GetFormattedWithDigits(language, Messages.InvalidLength, LenMin, allowedLength);
        await Client.ReplyMessageAsync(invalidLengthText, message, cancellationToken);
        return StateTrigger.InvalidData;
    }

    private static string TrimWhitespaces(string description)
    {
        var trimmed = Regex.Replace(description, @"[ \t]{2,}", " ");
        return Regex.Replace(trimmed, @"(\r?\n){3,}", "\n\n");
    }
}