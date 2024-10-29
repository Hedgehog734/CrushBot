using System.Text;
using CrushBot.Application.Interfaces;
using CrushBot.Application.Models;
using CrushBot.Core.Enums;
using CrushBot.Core.Localization;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CrushBot.Application.Services;

public class AccountService(ICityManager manager) : IAccountService
{
    public async Task<List<IAlbumInputMedia>> GetProfile(BotUserDto user, Language language,
        string? status = null)
    {
        var media = new List<IAlbumInputMedia>();

        if (user.VideoId != null)
        {
            var video = new InputMediaVideo(InputFile.FromFileId(user.VideoId))
            {
                Caption = await GetCaption(user, language, status),
                ParseMode = ParseMode.MarkdownV2
            };

            media.Add(video);
        }
        else
        {
            var photos = user.PhotoIds.Select(x => new InputMediaPhoto(InputFile.FromFileId(x))).ToList();

            var photo = photos.First();
            photo.Caption = await GetCaption(user, language, status);
            photo.ParseMode = ParseMode.MarkdownV2;

            media.AddRange(photos);
        }

        return media;
    }

    private async Task<string> GetCaption(BotUserDto user, Language language, string? status)
    {
        var builder = new StringBuilder();

        var cityName = await manager.GetAddCityName(user.CityId!, language);
        cityName = Markdown.Escape(cityName).EnsureDirection(language);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusText = $"{status.EnsureDirection(language)}";
            builder.AppendLine(statusText);
            builder.AppendLine();
        }

        var name = $"*{GetName(user)}*";
        builder.AppendLine(name);

        var age = user.Age!.Value;
        var ageString = age.ToString().ConvertDigits(language).EnsureDirection(language);

        var caption = StringExtensions.Join(language, ageString, cityName);
        builder.Append(caption);

        if (!string.IsNullOrWhiteSpace(user.Description))
        {
            builder.AppendLine();
            builder.AppendLine();
            builder.Append(user.Description);
        }

        return builder.ToString();
    }

    private static string GetName(BotUserDto user)
    {
        var builder = new StringBuilder();
        var isSubscribed = user is { IsSubscribed: true, ShowEmoji: true };

        if (isSubscribed)
        {
            builder.Append($"{Messages.Subscribed} ");
        }

        var name = Markdown.Escape(user.Name);
        return builder.Append(name).ToString();
    }
}