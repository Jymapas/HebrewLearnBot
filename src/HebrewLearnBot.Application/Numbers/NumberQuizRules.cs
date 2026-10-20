using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Application.Numbers;

public static class NumberQuizRules
{
    public static bool IsTextInput(NumberQuizMode mode)
    {
        ValidateMode(mode);
        return mode is NumberQuizMode.HebrewToNumberInput or NumberQuizMode.TransliterationToNumberInput;
    }

    public static int MaxValue(NumberQuizMode mode)
    {
        ValidateMode(mode);
        return mode == NumberQuizMode.NumberToHebrewSixOptions ? 10 : 99;
    }

    public static int OptionCount(NumberQuizMode mode)
    {
        return IsTextInput(mode) ? 0 : mode == NumberQuizMode.NumberToHebrewSixOptions ? 6 : 4;
    }

    public static bool TryParseAnswer(string? text, out int value)
    {
        value = 0;
        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 2)
        {
            return false;
        }

        foreach (var character in trimmed)
        {
            if (character is < '0' or > '9')
            {
                value = 0;
                return false;
            }

            value = value * 10 + character - '0';
        }

        return true;
    }

    private static void ValidateMode(NumberQuizMode mode)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }
}
