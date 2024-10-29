using System.Globalization;
using System.Resources;
using CrushBot.Core.Enums;
using CrushBot.Core.Localization;

namespace CrushBot.Application.Localization;

public class Localizer(string baseName) : ILocalizer
{
    private readonly ResourceManager _resourceManager = new(baseName, typeof(Localizer).Assembly);

    public string GetString(Language language, string key, string? initial = null)
    {
        var culture = GetCultureInfo(language, initial);
        var text = _resourceManager.GetString(key, culture);

        if (string.IsNullOrWhiteSpace(text))
        {
            var defaultCulture = GetCultureInfo(ILocalizer.DefaultLanguage);
            return _resourceManager.GetString(key, defaultCulture) ?? key;
        }

        return text;
    }

    public string GetDirected(Language language, string key)
    {
        return GetString(language, key).EnsureDirection(language);
    }

    public string GetFormatted(Language language, string key, params object[] args)
    {
        var format = GetString(language, key);
        return string.Format(format, args);
    }

    public string GetFormattedWithDigits(Language language, string key, params object[] args)
    {
        return GetFormatted(language, key, args).ConvertDigits(language);
    }

    private static CultureInfo GetCultureInfo(Language language, string? initial = null)
    {
        return language switch
        {
            Language.Arabic => new CultureInfo(IsoCodes.Arabic),
            Language.English => new CultureInfo(IsoCodes.English),
            Language.Filipino => new CultureInfo(IsoCodes.Filipino),
            Language.Hindi => new CultureInfo(IsoCodes.Hindi),
            Language.Indonesian => new CultureInfo(IsoCodes.Indonesian),
            Language.Kazakh => new CultureInfo(IsoCodes.Kazakh),
            Language.Persian => new CultureInfo(IsoCodes.Persian),
            Language.PortugueseBrazil => new CultureInfo(IsoCodes.PortugueseBrazil),
            Language.Russian => new CultureInfo(IsoCodes.Russian),
            Language.Spanish => new CultureInfo(IsoCodes.Spanish),
            Language.Turkish => new CultureInfo(IsoCodes.Turkish),
            Language.Ukrainian => new CultureInfo(IsoCodes.Ukrainian),
            Language.Uzbek => new CultureInfo(IsoCodes.Uzbek),
            Language.Vietnamese => new CultureInfo(IsoCodes.Vietnamese),
            _ => initial == null
                ? new CultureInfo(ILocalizer.DefaultCode)
                : new CultureInfo(initial)
        };
    }
}
