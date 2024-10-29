using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Core.Interfaces;

public interface ITelegramClient
{
    ITelegramBotClient GetBaseClient();

    Task<User?> GetMeAsync(CancellationToken cancellationToken);

    Task<string?> GetUserName(long userId, CancellationToken cancellationToken);

    Task SendMessageAsync(string text, Message message, CancellationToken cancellationToken = default,
        IReplyMarkup? keyboard = null, ParseMode parseMode = default, LinkPreviewOptions? linkOptions = null);

    Task SendMessageAsync(string text, long chatId, CancellationToken cancellationToken = default,
        IReplyMarkup? keyboard = null, ParseMode parseMode = default);

    Task ReplyMessageAsync(string text, Message message, CancellationToken cancellationToken,
        ParseMode parseMode = default);

    Task<ChatMember> GetChatMemberAsync(long channelId, long userId,
        CancellationToken cancellationToken);

    Task SendMediaGroupAsync(Chat chat, IEnumerable<IAlbumInputMedia> media,
        bool protectContent, CancellationToken cancellationToken);

    Task SendMediaGroupAsync(long chatId, IEnumerable<IAlbumInputMedia> media,
        bool protectContent, CancellationToken cancellationToken);
}
