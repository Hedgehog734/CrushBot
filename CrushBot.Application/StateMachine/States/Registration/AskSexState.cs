using CrushBot.Application.Interfaces;
using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Registration;

public class AskSexState(
    IAgeService service,
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<AskSexState> logger)
    : BaseState(client, localizer, logger)
{
    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;

        var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };
        keyboard.AddNewRow(GetMaleButton(language), GetFemaleButton(language));

        keyboard = ReverseKeyboardIfRtl(keyboard, language);

        var text = Localizer.GetString(language, Messages.AskSex);
        await Client.SendMessageAsync(text, message, cancellationToken, keyboard);
    }

    protected override Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;
        var text = message.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var maleText = GetMaleButton(language);

            if (text.Equals(maleText, StringComparison.OrdinalIgnoreCase))
            {
                user.Sex = Sex.Male;
                return SetAgeFilter(user);
            }

            var femaleText = GetFemaleButton(language);

            if (text.Equals(femaleText, StringComparison.OrdinalIgnoreCase))
            {
                user.Sex = Sex.Female;
                return SetAgeFilter(user);
            }
        }

        return Task.FromResult(StateTrigger.InvalidData);
    }

    public override UserState State => UserState.AskSex;

    private Task<StateTrigger> SetAgeFilter(BotUserDto user)
    {
        try
        {
            service.ResetUserFilter(user);
            return Task.FromResult(StateTrigger.DataEntered);
        }
        catch (ArgumentNullException)
        {
            return Task.FromResult(StateTrigger.DataNotFound);
        }
    }

    private string GetMaleButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.Male);
    }

    private string GetFemaleButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.Female);
    }
}