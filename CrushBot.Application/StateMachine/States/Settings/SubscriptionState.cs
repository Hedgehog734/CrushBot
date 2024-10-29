using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.States.Common;
using CrushBot.Core.Enums;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using CrushBot.Core.Settings;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Application.StateMachine.States.Settings;

public class SubscriptionState(
    ITelegramClient client,
    ISubscription settings,
    ILocalizer localizer,
    ILogger<SubscriptionState> logger)
    : BaseWithButtonsState(client, localizer, logger)
{

    protected override async Task OnEnterCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var language = user.Language;
        var isSubscribed = user.IsSubscribed;

        var replyKeyboard = new ReplyKeyboardMarkup(true) { IsPersistent = true };

        if (isSubscribed)
        {
            replyKeyboard.AddNewRow(user.ShowEmoji
                ? GetHideEmojiButton(language)
                : GetShowEmojiButton(language));
        }

        replyKeyboard.AddNewRow(GetBackButton(language));

        var titleKey = isSubscribed ? Messages.SubActive : Messages.SubInactive;
        var title = Localizer.GetFormatted(language, titleKey);

        var features = Localizer.GetFormatted(language, Messages.Features);

        var replyText = $"{title}\n{features}\n\n";

        if (isSubscribed)
        {
            replyText += Localizer.GetFormatted(language, Messages.Management, settings.Management);
        }

        var linkOptions = new LinkPreviewOptions { IsDisabled = true };
        await Client.SendMessageAsync(replyText, message, cancellationToken, replyKeyboard,
            ParseMode.MarkdownV2, linkOptions);

        if (!isSubscribed)
        {
            var inlineKeyboard = new InlineKeyboardMarkup()
                .AddNewRow(InlineKeyboardButton.WithUrl(GetSubscribeYearlyButton(language), settings.YearlyLink))
                .AddNewRow(InlineKeyboardButton.WithUrl(GetSubscribeMonthlyButton(language), settings.MonthlyLink));

            var inlineText = Localizer.GetFormattedWithDigits(language, Messages.Subscribe, settings.YearlyDiscount);
            await Client.SendMessageAsync(inlineText, message, cancellationToken, inlineKeyboard, ParseMode.MarkdownV2);
        }
    }

    protected override Task<StateTrigger> HandleCoreAsync(BotUserDto user, Message message,
        CancellationToken cancellationToken)
    {
        var text = message.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(text))
        {
            var language = user.Language;
            var backText = GetBackButton(language);

            if (text.Equals(backText, StringComparison.OrdinalIgnoreCase))
            {
                var result = user.IsSubscribed ? StateTrigger.OptionOne : StateTrigger.OptionTwo;
                return Task.FromResult(result);
            }

            if (user.IsSubscribed)
            {
                var enteredResult = Task.FromResult(StateTrigger.DataEntered);
                var showText = GetShowEmojiButton(language);

                if (text.Equals(showText, StringComparison.OrdinalIgnoreCase))
                {
                    user.ShowEmoji = true;
                    return enteredResult;
                }

                var hideText = GetHideEmojiButton(language);

                if (text.Equals(hideText, StringComparison.OrdinalIgnoreCase))
                {
                    user.ShowEmoji = false;
                    return enteredResult;
                }
            }
        }

        return Task.FromResult(StateTrigger.InvalidData);
    }

    public override UserState State => UserState.Subscription;

    private string GetSubscribeMonthlyButton(Language language)
    {
        return Localizer.GetFormattedWithDigits(language, Buttons.SubscribeMonthly, settings.MonthlyPrice)
            .EnsureDirection(language);
    }

    private string GetSubscribeYearlyButton(Language language)
    {
        var price = settings.YearlyPrice / 100 * (100 - settings.YearlyDiscount); // todo real price if not first time
        price = Math.Round(price, 1);

        return Localizer.GetFormattedWithDigits(language, Buttons.SubscribeYearly, price,
                settings.YearlyDiscount).EnsureDirection(language);
    }

    private string GetShowEmojiButton(Language language)
    {
        return Localizer.GetString(language, Buttons.ShowEmoji).EnsureDirection(language);
    }

    private string GetHideEmojiButton(Language language)
    {
        return Localizer.GetString(language, Buttons.HideEmoji).EnsureDirection(language);
    }
}