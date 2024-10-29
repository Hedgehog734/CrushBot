using CrushBot.Application.Models;

namespace CrushBot.Application.Interfaces;

public interface IAgeService
{
    (int, int) GetAllowedRange(BotUserDto user);

    (int, int) GetSubscriptionRange(BotUserDto user);

    void ResetUserFilter(BotUserDto user);

    bool SetUserFilter(string range, BotUserDto user);

    bool IsUserValid(BotUserDto user, bool throwException = false);
}