using CrushBot.Core.Entities;
using Telegram.Bot.Types;

namespace CrushBot.Core.Interfaces;

public interface ISubscriptionService
{
    Task ProcessMemberAsync(ChatMemberUpdated member);

    Task<bool> IsUserSubscribedAsync(long userId, ITelegramClient client,
        CancellationToken cancellationToken = default);

    BotUser CutDataBySubscription(BotUser user);
}