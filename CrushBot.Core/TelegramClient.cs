using CrushBot.Core.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrushBot.Core;

public class TelegramClient(ITelegramBotClient client) : ITelegramClient
{
    public ITelegramBotClient GetBaseClient()
    {
        return client;
    }

    public async Task<User?> GetMeAsync(CancellationToken cancellationToken)
    {
        return await client.GetMeAsync(cancellationToken);
    }

    public async Task<string?> GetUserName(long userId, CancellationToken cancellationToken)
    {
        var chat = await client.GetChatAsync(userId, cancellationToken);
        return chat.Username;
    }

    public async Task SendMessageAsync(string text, Message message, CancellationToken cancellationToken = default,
        IReplyMarkup? keyboard = null, ParseMode parseMode = default, LinkPreviewOptions? linkOptions = null)
    {
        await SendTextMessageAsync(message.Chat.Id, text, parseMode, keyboard, linkOptions, cancellationToken);
    }

    public async Task SendMessageAsync(string text, long chatId, CancellationToken cancellationToken = default,
        IReplyMarkup? keyboard = null, ParseMode parseMode = default)
    {
        await SendTextMessageAsync(chatId, text, parseMode, keyboard, null, cancellationToken);
    }

    public async Task ReplyMessageAsync(string text, Message message, CancellationToken cancellationToken,
        ParseMode parseMode = default)
    {
        await client.SendTextMessageAsync(message.Chat, text, replyParameters: message,
            parseMode: parseMode, cancellationToken: cancellationToken);
    }

    public async Task<ChatMember> GetChatMemberAsync(long channelId, long userId,
        CancellationToken cancellationToken)
    {
        return await client.GetChatMemberAsync(channelId, userId,
            cancellationToken: cancellationToken);
    }

    public async Task SendMediaGroupAsync(Chat chat, IEnumerable<IAlbumInputMedia> media,
        bool protectContent, CancellationToken cancellationToken)
    {
        await client.SendMediaGroupAsync(chat, media, protectContent: protectContent,
            cancellationToken: cancellationToken);
    }

    public async Task SendMediaGroupAsync(long chatId, IEnumerable<IAlbumInputMedia> media,
        bool protectContent, CancellationToken cancellationToken)
    {
        await client.SendMediaGroupAsync(chatId, media, protectContent: protectContent,
            cancellationToken: cancellationToken);
    }

    private async Task SendTextMessageAsync(long chatId, string text, ParseMode parseMode,
        IReplyMarkup? keyboard, LinkPreviewOptions? linkOptions, CancellationToken cancellationToken)
    {
        await client.SendTextMessageAsync(chatId, text, parseMode: parseMode, replyMarkup: keyboard,
            cancellationToken: cancellationToken, linkPreviewOptions: linkOptions);
    }
}
