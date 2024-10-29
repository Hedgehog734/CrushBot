using CrushBot.Application.Models;
using CrushBot.Application.Services.AgeService;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Registration;

public class AskAgeState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<AskAgeState> logger)
    : BaseState(client, localizer, logger)
{
    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        IReplyMarkup keyboard;
        var age = user.Age;

        if (age.HasValue)
        {
            keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true }
                .AddNewRow(age.Value.ToString().ConvertDigits(user.Language));
        }
        else
        {
            keyboard = new ForceReplyMarkup();
        }

        var text = Localizer.GetString(user.Language, Messages.AskAge);
        await Client.SendMessageAsync(text, message, cancellationToken, keyboard);
    }

    protected override async Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        if (message.Text.TryParseNumber(out var age) &&
            age is >= AgeResolver.MinAge and <= AgeResolver.MaxAge)
        {
            user.Age = age;
            return StateTrigger.DataEntered;
        }

        var text = Localizer.GetFormattedWithDigits(user.Language, Messages.AgeValidation,
            AgeResolver.MinAge, AgeResolver.MaxAge);

        await Client.ReplyMessageAsync(text, message, cancellationToken);
        return StateTrigger.InvalidData;
    }

    public override UserState State => UserState.AskAge;
}