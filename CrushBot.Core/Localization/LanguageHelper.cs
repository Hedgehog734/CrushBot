using CrushBot.Core.Entities;
using CrushBot.Core.Enums;

namespace CrushBot.Core.Localization;

public static class LanguageHelper
{
    public static readonly Dictionary<Language, string> LanguageToCode = new()
    {
        { Language.Arabic, IsoCodes.Arabic },
        { Language.English, IsoCodes.English },
        { Language.Filipino, IsoCodes.Filipino },
        { Language.Hindi, IsoCodes.Hindi },
        { Language.Indonesian, IsoCodes.Indonesian },
        { Language.Kazakh, IsoCodes.Kazakh },
        { Language.Persian, IsoCodes.Persian },
        { Language.PortugueseBrazil, IsoCodes.PortugueseBrazil },
        { Language.Russian, IsoCodes.Russian },
        { Language.Spanish, IsoCodes.Spanish },
        { Language.Turkish, IsoCodes.Turkish },
        { Language.Ukrainian, IsoCodes.Ukrainian },
        { Language.Uzbek, IsoCodes.Uzbek },
        { Language.Vietnamese, IsoCodes.Vietnamese }
    };

    public static readonly Dictionary<string, Language> CodeToLanguage = LanguageToCode
        .ToDictionary(pair => pair.Value, pair => pair.Key);

    public static string GetCode(Language language)
    {
        LanguageToCode.TryGetValue(language, out var tag);
        return tag ?? ILocalizer.DefaultCode;
    }

    public static Language GetLanguage(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return ILocalizer.DefaultLanguage;
        }

        CodeToLanguage.TryGetValue(code, out var language);

        return language == Language.None
            ? ILocalizer.DefaultLanguage
            : language;
    }

    public static string GetDisplayName(Language language) => LanguageDisplayNames[language];

    public static Language FromDisplayName(string displayName)
    {
        return LanguageDisplayNames.FirstOrDefault(pair => pair.Value == displayName).Key;
    }

    public static Language ResolveLanguage(BotUser user, string? initial)
    {
        return user.Language == Language.None
            ? GetLanguage(initial)
            : user.Language;
    }

    private static readonly Dictionary<Language, string> LanguageDisplayNames = new()
    {
        { Language.Arabic, "العربية 🇸🇦" },
        { Language.English, "English 🇬🇧" },
        { Language.Filipino, "Filipino 🇵🇭" },
        { Language.Hindi, "हिन्दी 🇮🇳" },
        { Language.Indonesian, "Bahasa Indonesia 🇮🇩" },
        { Language.Kazakh, "Қазақша 🇰🇿" },
        { Language.Persian, "فارسی 🇮🇷" },
        { Language.PortugueseBrazil, "Português 🇧🇷" },
        { Language.Russian, "Русский 🇷🇺" },
        { Language.Spanish, "Español 🇪🇸" },
        { Language.Turkish, "Türkçe 🇹🇷" },
        { Language.Ukrainian, "Українська 🇺🇦" },
        { Language.Uzbek, "Оʻzbek 🇺🇿" },
        { Language.Vietnamese, "Tiếng Việt 🇻🇳" }
    };
}