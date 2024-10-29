using CrushBot.Application.Interfaces;
using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.EditFilters;

public class ChangeAgeFilterState(
    IAgeService ageService,
    ITelegramClient client,
    ILocalizer localizer,
    ILogger<ChangeAgeFilterState> logger)
    : BaseWithButtonsState(client, localizer, logger)
{
    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;

        try
        {
            var placeholder = "00-00 / 00";
            placeholder = placeholder.ConvertDigits(language).EnsureDirection(language);

            var keyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true, InputFieldPlaceholder = placeholder };
            keyboard.AddNewRow(GetBackButton(language));

            var (minAllowed, maxAllowed) = ageService.GetAllowedRange(user);

            string text;

            if (user.IsSubscribed)
            {
                text = Localizer.GetFormattedWithDigits(language, Messages.AskAgeFilter, minAllowed, maxAllowed);
            }
            else
            {
                var (minSub, maxSub) = ageService.GetSubscriptionRange(user);

                text = Localizer.GetFormattedWithDigits(language, Messages.AskAgeFilterSub, minAllowed, maxAllowed,
                    minSub, maxSub);
            }

            await Client.SendMessageAsync(text, message, cancellationToken, keyboard, ParseMode.MarkdownV2);
        }
        catch (ArgumentNullException)
        {
            await HandleCoreAsync(user, message, cancellationToken);
        }
    }

    protected override async Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        if (!ageService.IsUserValid(user))
        {
            return StateTrigger.DataNotFound;
        }

        var language = user.Language;
        var range = message.Text?.Trim();
        var backText = GetBackButton(language);

        if (range!.Equals(backText, StringComparison.OrdinalIgnoreCase))
        {
            return StateTrigger.PreviousStep;
        }

        var isFilterSet = ageService.SetUserFilter(range, user);

        if (isFilterSet)
        {
            return StateTrigger.DataEntered;
        }

        var (min, max) = ageService.GetAllowedRange(user);
        var text = Localizer.GetFormattedWithDigits(language, Messages.AgeValidation, min, max);
        await Client.ReplyMessageAsync(text, message, cancellationToken);

        return StateTrigger.InvalidData;
    }

    public override UserState State => UserState.ChangeAgeFilter;
}