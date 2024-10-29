using System.Text;
using CrushBot.Application.Interfaces;
using CrushBot.Application.Models;
using CrushBot.Application.StateMachine.Extensions;
using CrushBot.Core.Interfaces;
using CrushBot.Core.Localization;
using Telegram.Bot.Types.Enums;

namespace CrushBot.Application.Services;

public class WeightService(ITelegramClient client, ILocalizer localizer)
    : IWeightService
{
    public const int SubscriptionWeight = ConsecutiveDayWeight * ConsecutiveDaysStreak;

    public const int ConsecutiveDaysStreak = 5;
    public const int ConsecutiveDayWeight = 10;

    public const int LastDayWeight = 5;
    public const int LastDaysStreak = 10;

    public async Task UpdateCurrentUserWeight(BotUserDto user)
    {
        var daysVisited = user.DaysVisited;
        user.UpdateCurrentUserWeight();

        if (!user.IsSubscribed)
        {
            if (user.DaysVisited > daysVisited)
            {
                string text;
                var language = user.Language;

                if (user.DaysVisited >= ConsecutiveDaysStreak)
                {
                    text = localizer.GetFormatted(language, Messages.BoostKeep, ConsecutiveDaysStreak);
                }
                else
                {
                    text = localizer.GetFormatted(language, Messages.BoostIncreased,
                        user.DaysVisited, ConsecutiveDaysStreak);
                }

                text = text.ConvertDigits(language);
                await client.SendMessageAsync(text, user.Id, parseMode: ParseMode.MarkdownV2);
            }
        }

        if (user.IsLowWeight)
        {
            user.IsLowWeight = false;
        }
    }

    public async Task UpdateFeedUserWeight(BotUserDto user)
    {
        user.UpdateFeedUserWeight();

        var lowerWeightBorder = user.IsSubscribed
            ? SubscriptionWeight
            : 0;

        if (!user.IsLowWeight && user.Weight <= lowerWeightBorder)
        {
            var language = user.Language;
            var builder = new StringBuilder();

            var text = localizer.GetString(language, Messages.LowWeight);
            builder.AppendLine(text.ConvertDigits(language));
            builder.AppendLine();

            text = localizer.GetString(language,
                user.IsSubscribed
                    ? Messages.LowWeightSub
                    : Messages.LowWeightNoSub);

            builder.Append(text);
            builder.Append(" ");

            text = localizer.GetString(language, Messages.Relevance);
            builder.Append(text);

            await client.SendMessageAsync(builder.ToString(), user.Id, parseMode: ParseMode.MarkdownV2);

            user.IsLowWeight = true;
        }
        else if (user.IsLowWeight)
        {
            user.IsLowWeight = false;
        }
    }
}