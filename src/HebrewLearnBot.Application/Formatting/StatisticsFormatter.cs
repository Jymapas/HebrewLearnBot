using System.Globalization;
using System.Text;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Formatting;

public static class StatisticsFormatter
{
    public static string Build(UserStatistics statistics)
    {
        var builder = new StringBuilder();

        builder.Append(BidiText.Line("📊 Статистика"));
        builder.Append('\n');
        builder.Append('\n');

        builder.Append(BidiText.Line($"Слов в словаре: {statistics.TotalWords.ToString(CultureInfo.InvariantCulture)}"));
        builder.Append('\n');
        builder.Append(BidiText.Line($"Знакомых: {statistics.SeenWords.ToString(CultureInfo.InvariantCulture)}"));
        builder.Append('\n');
        builder.Append(BidiText.Line($"Выучено: {statistics.LearnedWords.ToString(CultureInfo.InvariantCulture)}"));
        builder.Append('\n');
        builder.Append('\n');

        builder.Append(BidiText.Line($"Ответов: {statistics.TotalAnswers.ToString(CultureInfo.InvariantCulture)}"));
        builder.Append('\n');

        if (statistics.TotalAnswers == 0)
        {
            builder.Append(BidiText.Line("Точность: —"));
            builder.Append('\n');
            builder.Append('\n');
            builder.Append(BidiText.Line("Вы ещё не отвечали на вопросы."));

            return builder.ToString();
        }

        var percent = (int)Math.Round(
            statistics.CorrectAnswers * 100.0 / statistics.TotalAnswers,
            MidpointRounding.AwayFromZero);

        builder.Append(BidiText.Line($"Точность: {percent.ToString(CultureInfo.InvariantCulture)}%"));

        if (statistics.TopProblemWords.Count == 0)
        {
            return builder.ToString();
        }

        builder.Append('\n');
        builder.Append('\n');
        builder.Append(BidiText.Line("Чаще всего ошибаетесь:"));

        foreach (var word in statistics.TopProblemWords)
        {
            builder.Append('\n');
            builder.Append(BidiText.Line($"• {BidiText.Isolate(word.Hebrew)} — {word.TranslationRu}"));
        }

        return builder.ToString();
    }
}
