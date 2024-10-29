using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.EditFilters;

public class EditFiltersState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<EditFiltersState> logger)
    : BaseWithButtonsState(client, localizer, logger)
{
    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;
        var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };

        keyboard.AddNewRow(GetChangeAgeButton(language))
            .AddNewRow(GetChangeSexButton(language))
            .AddNewRow(GetBackButton(language));

        var text = Localizer.GetString(language, Messages.ChooseAction);
        await Client.SendMessageAsync(text, message, cancellationToken, keyboard);
    }

    protected override Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var text = message.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var language = user.Language;
            var ageButton = GetChangeAgeButton(language);

            if (text.Equals(ageButton, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(StateTrigger.OptionOne);
            }

            var sexButton = GetChangeSexButton(language);

            if (text.Equals(sexButton, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(StateTrigger.OptionTwo);
            }

            var backText = GetBackButton(language);

            if (text.Equals(backText, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(StateTrigger.PreviousStep);
            }
        }

        return Task.FromResult(StateTrigger.InvalidData);
    }

    public override UserState State => UserState.EditFilters;

    private string GetChangeAgeButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.ChangeAge);
    }

    private string GetChangeSexButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.ChangeSex);
    }
}