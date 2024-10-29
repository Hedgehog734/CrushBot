using CrushBot.Application.Interfaces;
using CrushBot.Application.Models;
using CrushBot.Core.Enums;
using CrushBot.Core.Localization;

namespace CrushBot.Application.Services.AgeService;

public class AgeService : IAgeService
{
    public (int, int) GetAllowedRange(BotUserDto user)
    {
        IsUserValid(user, true);

        var age = user.Age!.Value;
        var isMale = user.Sex == Sex.Male;

        if (user.IsSubscribed) 
        {
            return isMale
                ? AgeResolver.GetMaleAgeBoundary(age)
                : AgeResolver.GetFemaleAgeBoundary(age);
        }

        return isMale
            ? AgeResolver.GetMaleAgeSuggested(age)
            : AgeResolver.GetFemaleAgeSuggested(age);
    }

    public (int, int) GetSubscriptionRange(BotUserDto user)
    {
        IsUserValid(user, true);

        var age = user.Age!.Value;
        var isMale = user.Sex == Sex.Male;

        return isMale
            ? AgeResolver.GetMaleAgeBoundary(age)
            : AgeResolver.GetFemaleAgeBoundary(age);
    }

    public void ResetUserFilter(BotUserDto user)
    {
        var (min, max) = GetAllowedRange(user);
        user.Filter!.AgeAfter = min;
        user.Filter.AgeUntil = max;
    }

    public bool SetUserFilter(string range, BotUserDto user)
    {
        var (minAllowed, maxAllowed) = GetAllowedRange(user);
        var (min, max) = ParseAgeRange(range);

        if (min >= minAllowed && max <= maxAllowed)
        {
            user.Filter!.AgeAfter = min;
            user.Filter.AgeUntil = max;

            return true;
        }

        return false;
    }

    public bool IsUserValid(BotUserDto user, bool throwException = false)
    {
        var result = true;

        if (user.Sex == Sex.None)
        {
            result = false;

            if (throwException)
            {
                throw new ArgumentNullException(nameof(user.Sex));
            }
        }

        if (!user.Age.HasValue)
        {
            result = false;

            if (throwException)
            {
                throw new ArgumentNullException(nameof(user.Age));
            }
        }

        return result;
    }

    private static (int, int) ParseAgeRange(string text)
    {
        var parts = text.Split('-');

        if (parts.Length == 1
            && parts[0].TryParseNumber(out var age))
        {
            return (age, age);
        }

        if (parts.Length == 2 &&
            parts[0].TryParseNumber(out age) &&
            parts[1].TryParseNumber(out var endAge))
        {
            return (age, endAge);
        }

        return (0, 0);
    }
}