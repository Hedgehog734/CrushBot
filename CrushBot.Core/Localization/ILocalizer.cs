using CrushBot.Core.Enums;

namespace CrushBot.Core.Localization;

public interface ILocalizer
{
    public const Language DefaultLanguage = Language.English;
    public const string DefaultCode = IsoCodes.English;

    string GetString(Language language, string key, string? initial = null);

    string GetDirected(Language language, string key);

    string GetFormatted(Language language, string key, params object[] args);

    string GetFormattedWithDigits(Language language, string key, params object[] args);
}
