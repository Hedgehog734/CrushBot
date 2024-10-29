using CrushBot.Application.Models;
using CrushBot.Core.Enums;
using Telegram.Bot.Types;

namespace CrushBot.Application.Interfaces;

public interface IAccountService
{
    Task<List<IAlbumInputMedia>> GetProfile(BotUserDto user, Language language, string? status = null);
}