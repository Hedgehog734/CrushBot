using System.Text.RegularExpressions;
using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Registration;

public class AskNameState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<AskNameState> logger)
    : BaseState(client, localizer, logger)
{
    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        IReplyMarkup keyboard;

        var name = RemoveEmoji(message.From!.FirstName);
        name = TrimWhitespaces(name);

        if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(user.Name))
        {
            var replyKeyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };

            if (!string.IsNullOrWhiteSpace(name))
            {
                replyKeyboard.AddNewRow(name);
            }

            if (!string.IsNullOrWhiteSpace(user.Name) &&
                !name.Equals(user.Name, StringComparison.Ordinal))
            {
                replyKeyboard.AddNewRow(user.Name);
            }

            keyboard = replyKeyboard;
        }
        else
        {
            keyboard = new ForceReplyMarkup();
        }

        var text = Localizer.GetString(user.Language, Messages.AskName);
        await Client.SendMessageAsync(text, message, cancellationToken, keyboard);
    }

    protected override async Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        const int lenMin = 1;
        const int lenMax = 64;

        var name = RemoveEmoji(message.Text!);
        name = TrimWhitespaces(name);

        if (!string.IsNullOrWhiteSpace(name))
        {
            if (name.Length is >= lenMin and <= lenMax)
            {
                user.Name = name;
                return StateTrigger.DataEntered;
            }
        }

        var text = Localizer.GetFormattedWithDigits(user.Language, Messages.InvalidLength, lenMin, lenMax);
        await Client.ReplyMessageAsync(text, message, cancellationToken);
        return StateTrigger.InvalidData;
    }

    public override UserState State => UserState.AskName;

    private static string RemoveEmoji(string name)
    {
        return name.Replace(Messages.Subscribed, string.Empty).Trim();
    }

    private static string TrimWhitespaces(string name) => Regex.Replace(name, @"\s+", " ");
}