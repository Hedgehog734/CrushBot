using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Settings;

public class SettingsState(
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<SettingsState> logger)
    : BaseWithButtonsState(client, localizer, logger)
{
    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;
        var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };

        keyboard.AddNewRow(GetEditProfileButton(language))
            .AddNewRow(GetLanguageButton(language));

        if (user.IsSubscribed)
        {
            keyboard.AddNewRow(GetSubscriptionButton(language));
        }

        keyboard.AddNewRow(GetDeleteProfileButton(language))
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

            var editText = GetEditProfileButton(language);

            if (text.Equals(editText, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(StateTrigger.OptionOne);
            }

            if (user.IsSubscribed)
            {
                var subscriptionText = GetSubscriptionButton(language);

                if (text.Equals(subscriptionText, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(StateTrigger.OptionTwo);
                }
            }

            var languageText = GetLanguageButton(language);

            if (text.Equals(languageText, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(StateTrigger.OptionThree);
            }

            var deleteText = GetDeleteProfileButton(language);

            if (text.Equals(deleteText, StringComparison.OrdinalIgnoreCase))
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

    public override UserState State => UserState.Settings;

    private string GetEditProfileButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.EditProfile);
    }

    private string GetSubscriptionButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.Subscription);
    }

    private string GetLanguageButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.Language);
    }

    private string GetDeleteProfileButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.DeleteProfile);
    }
}