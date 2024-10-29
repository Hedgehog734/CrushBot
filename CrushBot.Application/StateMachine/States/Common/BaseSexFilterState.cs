using CrushBot.Application.Models;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Common;

public abstract class BaseSexFilterState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<BaseSexFilterState> logger)
    : BaseState(client, localizer, logger)
{
    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;
        var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };

        keyboard.AddNewRow(GetMaleButton(language), GetFemaleButton(language))
            .AddNewRow(GetAnyButton(language));

        keyboard = ReverseKeyboardIfRtl(keyboard, language);

        var text = Localizer.GetString(language, Messages.AskSexFilter);
        await Client.SendMessageAsync(text, message, cancellationToken, keyboard);
    }

    protected override Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var successResult = Task.FromResult(StateTrigger.DataEntered);
        var text = message.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var language = user.Language;
            var maleText = GetMaleButton(language);

            if (text.Equals(maleText, StringComparison.OrdinalIgnoreCase))
            {
                user.Filter!.Sex = Sex.Male;
                return successResult;
            }

            var femaleText = GetFemaleButton(language);

            if (text.Equals(femaleText, StringComparison.OrdinalIgnoreCase))
            {
                user.Filter!.Sex = Sex.Female;
                return successResult;
            }

            var anyText = GetAnyButton(language);

            if (text.Equals(anyText, StringComparison.OrdinalIgnoreCase))
            {
                user.Filter!.Sex = Sex.Any;
                return successResult;
            }
        }

        return Task.FromResult(StateTrigger.InvalidData);
    }

    private string GetMaleButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.MaleFilter);
    }

    private string GetFemaleButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.FemaleFilter);
    }

    private string GetAnyButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.AnyFilter);
    }
}