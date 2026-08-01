using System.Text;

namespace HebrewLearnBot.Domain.Text;

public static class HebrewText
{
    private const char HebrewLetterStart = 'א';
    private const char HebrewLetterEnd = 'ת';

    private const char HebrewBlockStart = '֐';
    private const char HebrewBlockEnd = '׿';

    // Огласовки и кантилляционные знаки
    private const char NiqqudRangeStart = '֑';
    private const char NiqqudRangeEnd = 'ֽ';
    private const char NiqqudRafe = 'ֿ';
    private const char NiqqudShinSinDotRangeStart = 'ׁ';
    private const char NiqqudShinSinDotRangeEnd = 'ׂ';
    private const char NiqqudUpperLowerDotRangeStart = 'ׄ';
    private const char NiqqudUpperLowerDotRangeEnd = 'ׅ';
    private const char NiqqudQamatsQatan = 'ׇ';

    public static bool ContainsHebrewLetter(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        foreach (var c in text)
        {
            if (c is >= HebrewLetterStart and <= HebrewLetterEnd)
            {
                return true;
            }
        }

        return false;
    }

    public static string StripNiqqud(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(text.Length);

        foreach (var c in text)
        {
            if (IsNiqqudOrCantillation(c))
            {
                continue;
            }

            builder.Append(c);
        }

        return builder.ToString();
    }

    public static bool IsLatinTransliteration(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        foreach (var c in text)
        {
            var isAllowed = c is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or ' ' or '\'' or '-';

            if (!isAllowed)
            {
                return false;
            }
        }

        return true;
    }

    public static bool ContainsHebrew(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        foreach (var c in text)
        {
            if (c is >= HebrewBlockStart and <= HebrewBlockEnd)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNiqqudOrCantillation(char c)
    {
        // U+05BE (макаф), U+05C0 (пасек), U+05C3 (соф-пасук), U+05C6 (нун хафуха) — пунктуация, сохраняем.
        if (c is >= NiqqudRangeStart and <= NiqqudRangeEnd)
        {
            return true;
        }

        if (c == NiqqudRafe)
        {
            return true;
        }

        if (c is >= NiqqudShinSinDotRangeStart and <= NiqqudShinSinDotRangeEnd)
        {
            return true;
        }

        if (c is >= NiqqudUpperLowerDotRangeStart and <= NiqqudUpperLowerDotRangeEnd)
        {
            return true;
        }

        if (c == NiqqudQamatsQatan)
        {
            return true;
        }

        return false;
    }
}
