using CrushBot.Application.Interfaces;
using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Profile;

public class ProfileState(
    IAccountService accountService,
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<ProfileState> logger)
    : BaseState(client, localizer, logger)
{
    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var media = await accountService.GetProfile(user, user.Language);
        await Client.SendMediaGroupAsync(message.Chat, media, false, cancellationToken);

        var language = user.Language;
        var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };

        keyboard.AddNewRow(GetViewProfilesButton(language))
            .AddNewRow(GetViewMatchesButton(language))
            .AddNewRow(GetFiltersButton(language))
            .AddNewRow();

        var isSubscribed = user.IsSubscribed;

        if (!isSubscribed)
        {
            keyboard.AddButton(GetSubscriptionButton(language));
        }

        keyboard.AddButton(GetSettingsButton(language));

        if (!isSubscribed)
        {
            keyboard = ReverseKeyboardIfRtl(keyboard, language);
        }

        var text = Localizer.GetString(language, Messages.Profile);
        await Client.SendMessageAsync(text, message, cancellationToken, keyboard);
    }

    protected override Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var text = message.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var language = user.Language;
            var viewProfilesText = GetViewProfilesButton(language);

            if (text.Equals(viewProfilesText, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(StateTrigger.OptionOne);
            }

            var viewMatchesText = GetViewMatchesButton(language);

            if (text.Equals(viewMatchesText, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(StateTrigger.OptionTwo);
            }

            var filtersText = GetFiltersButton(language);

            if (text.Equals(filtersText, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(StateTrigger.OptionThree);
            }

            var settingsText = GetSettingsButton(language);

            if (text.Equals(settingsText, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(StateTrigger.OptionFour);
            }

            if (!user.IsSubscribed)
            {
                var subscriptionText = GetSubscriptionButton(language);

                if (text.Equals(subscriptionText, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(StateTrigger.OptionFive);
                }
            }
        }

        return Task.FromResult(StateTrigger.InvalidData);
    }

    public override UserState State => UserState.Profile;

    private string GetViewProfilesButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.ViewProfiles);
    }

    private string GetViewMatchesButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.ViewMatches);
    }

    private string GetFiltersButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.Filters);
    }

    private string GetSubscriptionButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.Subscription);
    }

    private string GetSettingsButton(Language language)
    {
        return Localizer.GetDirected(language, Buttons.Settings);
    }
}