using System.Text;
using System.Text.RegularExpressions;

namespace HebrewLearnBot.Domain.Text;

public static class TextNormalizer
{
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    public static string NormalizeHebrew(string? text)
    {
        var stripped = HebrewText.StripNiqqud(text);
        var normalized = stripped.Normalize(NormalizationForm.FormC).Trim();

        return CollapseWhitespace(normalized);
    }

    public static string NormalizeRussian(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var normalized = text.Trim().ToLowerInvariant();
        normalized = normalized.Replace('ё', 'е');

        return CollapseWhitespace(normalized);
    }

    private static string CollapseWhitespace(string text)
    {
        return WhitespaceRegex.Replace(text, " ");
    }
}
