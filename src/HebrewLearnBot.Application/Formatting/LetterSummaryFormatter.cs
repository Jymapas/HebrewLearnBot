using System.Globalization;
using System.Text;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Formatting;

public static class LetterSummaryFormatter
{
    public static string Build(LetterQuizSummary summary)
    {
        var builder = new StringBuilder();

        builder.Append(BidiText.Line("🏁 Тренировка завершена"));
        builder.Append('\n');
        builder.Append('\n');

        if (summary.AskedCount == 0)
        {
            builder.Append(BidiText.Line("Вы не ответили ни на один вопрос."));
            return builder.ToString();
        }

        var percent = (int)Math.Round(summary.CorrectCount * 100.0 / summary.AskedCount, MidpointRounding.AwayFromZero);

        builder.Append(BidiText.Line(
            $"Отвечено: {summary.AskedCount.ToString(CultureInfo.InvariantCulture)} · " +
            $"Верно: {summary.CorrectCount.ToString(CultureInfo.InvariantCulture)} " +
            $"({percent.ToString(CultureInfo.InvariantCulture)}%)"));

        builder.Append('\n');
        builder.Append('\n');

        // Одна и та же буква может попасть в WrongLetters несколько раз за тренировку — выводим один раз.
        var distinctWrongLetters = summary.WrongLetters
            .GroupBy(letter => letter.LetterId)
            .Select(group => group.First())
            .ToList();

        if (distinctWrongLetters.Count == 0)
        {
            builder.Append(BidiText.Line("Ни одной ошибки, отлично! 🎉"));
            return builder.ToString();
        }

        builder.Append(BidiText.Line("Стоит повторить:"));

        foreach (var letter in distinctWrongLetters)
        {
            builder.Append('\n');
            builder.Append(BidiText.Line($"• {BidiText.Isolate(letter.Glyph)} — {letter.Name}"));
        }

        return builder.ToString();
    }
}
