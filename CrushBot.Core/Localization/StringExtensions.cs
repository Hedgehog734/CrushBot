using CrushBot.Core.Enums;

namespace CrushBot.Core.Localization;

public static class StringExtensions
{
    public static string EnsureDirection(this string text, Language language)
    {
        return language is Language.Arabic or Language.Persian
            ? $"‏{text}"
            : $"‎{text}";
    }

    public static string EnsureLtr(this string text)
    {
        return $"‎{text}";
    }

    public static string ConvertDigits(this string text, Language language)
    {
        return language switch
        {
            Language.Persian => text.Replace('0', '\u06f0')
                .Replace('1', '\u06f1')
                .Replace('2', '\u06f2')
                .Replace('3', '\u06f3')
                .Replace('4', '\u06f4')
                .Replace('5', '\u06f5')
                .Replace('6', '\u06f6')
                .Replace('7', '\u06f7')
                .Replace('8', '\u06f8')
                .Replace('9', '\u06f9')
                .Replace(',', '،'),
            Language.Arabic => text.Replace('0', '\u0660')
                .Replace('1', '\u0661')
                .Replace('2', '\u0662')
                .Replace('3', '\u0663')
                .Replace('4', '\u0664')
                .Replace('5', '\u0665')
                .Replace('6', '\u0666')
                .Replace('7', '\u0667')
                .Replace('8', '\u0668')
                .Replace('9', '\u0669')
                .Replace(',', '،'),
            _ => text
        };
    }

    public static bool TryParseNumber(this string? text, out int result)
    {
        var converted = text?.Replace('\u06f0', '0')
            .Replace('\u06f1', '1')
            .Replace('\u06f2', '2')
            .Replace('\u06f3', '3')
            .Replace('\u06f4', '4')
            .Replace('\u06f5', '5')
            .Replace('\u06f6', '6')
            .Replace('\u06f7', '7')
            .Replace('\u06f8', '8')
            .Replace('\u06f9', '9')
            .Replace('\u0660', '0')
            .Replace('\u0661', '1')
            .Replace('\u0662', '2')
            .Replace('\u0663', '3')
            .Replace('\u0664', '4')
            .Replace('\u0665', '5')
            .Replace('\u0666', '6')
            .Replace('\u0667', '7')
            .Replace('\u0668', '8')
            .Replace('\u0669', '9');

        return int.TryParse(converted, out result);
    }

    public static string AddAtSign(this string text)
    {
        return $"@{text}".EnsureLtr();
    }

    public static string NormalizeCityTitle(this string text, Language language)
    {
        text = text.Trim();
        var words = text.Split(',');
        var isArabic = language is Language.Arabic or Language.Persian;

        for (var i = 0; i < words.Length; i++)
        {
            words[i] = isArabic
            ? '‏' + words[i].Trim().ConvertDigits(language)
            : '‎' + words[i].Trim().ConvertDigits(language);
        }

        return Join(language, words.ToArray<object>());
    }

    public static string Join(Language language, params object[] values)
    {
        var separator = language is Language.Arabic or Language.Persian ? "، " : ", ";
        return string.Join(separator, values);
    }
}