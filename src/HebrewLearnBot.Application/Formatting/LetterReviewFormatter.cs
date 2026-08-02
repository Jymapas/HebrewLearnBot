using System.Globalization;
using System.Text;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Formatting;

public static class LetterReviewFormatter
{
    public static string Build(
        LetterBrief correctLetter,
        bool isCorrect,
        string? chosenLabel,
        int askedCount,
        int correctCount)
    {
        var builder = new StringBuilder();

        builder.Append(BidiText.Line(isCorrect ? "✅ Верно!" : "❌ Неверно"));

        if (!isCorrect && !string.IsNullOrEmpty(chosenLabel))
        {
            builder.Append('\n');
            builder.Append(BidiText.Line($"Вы выбрали: {chosenLabel}"));
        }

        builder.Append('\n');
        builder.Append('\n');
        builder.Append(BidiText.Line($"{BidiText.Isolate(correctLetter.Glyph)} — {correctLetter.Name}"));

        builder.Append('\n');
        builder.Append('\n');
        builder.Append(BidiText.Line(
            $"📊 {correctCount.ToString(CultureInfo.InvariantCulture)} из {askedCount.ToString(CultureInfo.InvariantCulture)}"));

        return builder.ToString();
    }
}
