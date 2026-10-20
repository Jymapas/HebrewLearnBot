using System.Globalization;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Formatting;

public static class NumberStatisticsFormatter
{
    public static string Build(NumberStatistics statistics)
    {
        var accuracy = statistics.TotalAnswers == 0
            ? "—"
            : Math.Round(statistics.CorrectAnswers * 100.0 / statistics.TotalAnswers, MidpointRounding.AwayFromZero)
                .ToString(CultureInfo.InvariantCulture) + "%";
        var lines = new List<string>
        {
            "🔢 Числа",
            $"Чисел в каталоге: {statistics.TotalNumbers.ToString(CultureInfo.InvariantCulture)}",
            $"Знакомых: {statistics.SeenNumbers.ToString(CultureInfo.InvariantCulture)}",
            $"Выучено: {statistics.LearnedNumbers.ToString(CultureInfo.InvariantCulture)}",
            "",
            $"Ответов: {statistics.TotalAnswers.ToString(CultureInfo.InvariantCulture)}",
            $"Точность: {accuracy}"
        };
        if (statistics.TotalAnswers == 0)
        {
            lines.Add("Вы ещё не отвечали на вопросы.");
        }

        return string.Join("\n", lines.Select(BidiText.Line));
    }
}
