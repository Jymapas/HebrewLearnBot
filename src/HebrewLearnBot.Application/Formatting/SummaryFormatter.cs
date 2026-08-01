using System.Globalization;
using System.Text;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Formatting;

public static class SummaryFormatter
{
    public static string Build(QuizSummary summary)
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

        // Одно и то же слово может попасть в WrongWords несколько раз за тренировку — выводим один раз.
        var distinctWrongWords = summary.WrongWords
            .GroupBy(word => word.WordId)
            .Select(group => group.First())
            .ToList();

        builder.Append('\n');
        builder.Append('\n');

        if (distinctWrongWords.Count == 0)
        {
            builder.Append(BidiText.Line("Ни одной ошибки, отлично! 🎉"));
            return builder.ToString();
        }

        builder.Append(BidiText.Line("Стоит повторить:"));

        foreach (var word in distinctWrongWords)
        {
            builder.Append('\n');
            builder.Append(BidiText.Line($"• {BidiText.Isolate(word.Hebrew)} — {word.TranslationRu}"));
        }

        return builder.ToString();
    }
}
