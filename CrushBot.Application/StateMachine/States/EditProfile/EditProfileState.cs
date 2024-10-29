using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.EditProfile;

public class EditProfileState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<EditProfileState> logger)
    : BaseWithButtonsState(client, localizer, logger)
{
    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;
        var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };

        keyboard.AddNewRow(GetChangeAllButton(language))
            .AddNewRow(GetChangeCityButton(language))
            .AddNewRow(GetChangeMediaButton(language))
            .AddNewRow(GetChangeDescriptionButton(language))
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
            var allButton = GetChangeAllButton(language);

            if (text.Equals(allButton, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(StateTrigger.OptionOne);
            }

            var cityButton = GetChangeCityButton(language);

            if (text.Equals(cityButton, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(StateTrigger.OptionTwo);
            }

            var mediaButton = GetChangeMediaButton(language);

            if (text.Equals(mediaButton, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(StateTrigger.OptionThree);
            }

            var descriptionButton = GetChangeDescriptionButton(language);

            if (text.Equals(descriptionButton, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(StateTrigger.OptionFour);
            }

            var backText = GetBackButton(language);

            if (text.Equals(backText, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(StateTrigger.PreviousStep);
            }
        }

        return Task.FromResult(StateTrigger.InvalidData);
    }

    public override UserState State => UserState.EditProfile;

    private string GetChangeAllButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.ChangeAll);
    }

    private string GetChangeCityButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.ChangeCity);
    }

    private string GetChangeMediaButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.ChangeMedia);
    }

    private string GetChangeDescriptionButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.ChangeDescription);
    }
}