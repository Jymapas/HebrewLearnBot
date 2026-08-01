using System.Text;
using HebrewLearnBot.Domain.Text;

namespace HebrewLearnBot.Application.Formatting;

public static class BidiText
{
    public const char LeftToRightMark = '‎';

    private const int MaxButtonLength = 48;

    // Оборачивает ивритский фрагмент маркерами LRM с обеих сторон, чтобы Telegram
    // не разворачивал соседний текст и не зеркалировал скобки/пунктуацию.
    public static string Isolate(string? hebrew)
    {
        if (string.IsNullOrEmpty(hebrew))
        {
            return string.Empty;
        }

        if (!ContainsHebrew(hebrew))
        {
            return hebrew;
        }

        return LeftToRightMark + hebrew + LeftToRightMark;
    }

    // Фиксирует базовое направление абзаца как LTR, вне зависимости от того, какой
    // символ идёт первым (иврит, эмодзи, цифра и т.д.).
    public static string Line(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text[0] == LeftToRightMark)
        {
            return text;
        }

        return LeftToRightMark + text;
    }

    public static string ForButton(string? label)
    {
        var line = Line(label);

        if (line.Length == 0)
        {
            return string.Empty;
        }

        // Ведущий LRM не входит в видимую длину подписи и должен пережить обрезку.
        var hasLeadingMark = line[0] == LeftToRightMark;
        var contentStart = hasLeadingMark ? 1 : 0;
        var visibleLength = line.Length - contentStart;

        if (visibleLength <= MaxButtonLength)
        {
            return line;
        }

        var truncated = line.Substring(contentStart, MaxButtonLength) + "…";

        return hasLeadingMark ? LeftToRightMark + truncated : truncated;
    }

    public static string Join(string separator, params string?[] parts)
    {
        var builder = new StringBuilder();

        for (var i = 0; i < parts.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(separator);
            }

            var part = parts[i] ?? string.Empty;
            builder.Append(ContainsHebrew(part) ? Isolate(part) : part);
        }

        return builder.ToString();
    }

    public static bool ContainsHebrew(string? text)
    {
        return HebrewText.ContainsHebrew(text);
    }
}
