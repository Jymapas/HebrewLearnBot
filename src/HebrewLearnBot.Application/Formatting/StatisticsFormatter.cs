using System.Globalization;
using System.Text;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Formatting;

public static class StatisticsFormatter
{
    public static string Build(UserStatistics wordStatistics, LetterStatistics letterStatistics)
    {
        var builder = new StringBuilder();

        builder.Append(BidiText.Line("📊 Статистика"));
        builder.Append('\n');
        builder.Append('\n');

        builder.Append(BidiText.Line("📖 Слова"));
        builder.Append('\n');
        AppendWordSection(builder, wordStatistics);

        builder.Append('\n');
        builder.Append('\n');

        builder.Append(BidiText.Line("🔤 Алфавит"));
        builder.Append('\n');
        AppendLetterSection(builder, letterStatistics);

        return builder.ToString();
    }

    private static void AppendWordSection(StringBuilder builder, UserStatistics statistics)
    {
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
            builder.Append(BidiText.Line("Вы ещё не отвечали на вопросы."));
            return;
        }

        var percent = (int)Math.Round(
            statistics.CorrectAnswers * 100.0 / statistics.TotalAnswers,
            MidpointRounding.AwayFromZero);

        builder.Append(BidiText.Line($"Точность: {percent.ToString(CultureInfo.InvariantCulture)}%"));

        if (statistics.TopProblemWords.Count == 0)
        {
            return;
        }

        builder.Append('\n');
        builder.Append('\n');
        builder.Append(BidiText.Line("Чаще всего ошибаетесь:"));

        foreach (var word in statistics.TopProblemWords)
        {
            builder.Append('\n');
            builder.Append(BidiText.Line($"• {BidiText.Isolate(word.Hebrew)} — {word.TranslationRu}"));
        }
    }

    private static void AppendLetterSection(StringBuilder builder, LetterStatistics statistics)
    {
        builder.Append(BidiText.Line($"Букв в алфавите: {statistics.TotalLetters.ToString(CultureInfo.InvariantCulture)}"));
        builder.Append('\n');
        builder.Append(BidiText.Line($"Знакомых: {statistics.SeenLetters.ToString(CultureInfo.InvariantCulture)}"));
        builder.Append('\n');
        builder.Append(BidiText.Line($"Выучено: {statistics.LearnedLetters.ToString(CultureInfo.InvariantCulture)}"));
        builder.Append('\n');
        builder.Append('\n');

        builder.Append(BidiText.Line($"Ответов: {statistics.TotalAnswers.ToString(CultureInfo.InvariantCulture)}"));
        builder.Append('\n');

        if (statistics.TotalAnswers == 0)
        {
            builder.Append(BidiText.Line("Точность: —"));
            builder.Append('\n');
            builder.Append(BidiText.Line("Вы ещё не отвечали на вопросы."));
            return;
        }

        var percent = (int)Math.Round(
            statistics.CorrectAnswers * 100.0 / statistics.TotalAnswers,
            MidpointRounding.AwayFromZero);

        builder.Append(BidiText.Line($"Точность: {percent.ToString(CultureInfo.InvariantCulture)}%"));

        if (statistics.TopProblemLetters.Count == 0)
        {
            return;
        }

        builder.Append('\n');
        builder.Append('\n');
        builder.Append(BidiText.Line("Чаще всего ошибаетесь:"));

        foreach (var letter in statistics.TopProblemLetters)
        {
            builder.Append('\n');
            builder.Append(BidiText.Line($"• {BidiText.Isolate(letter.Glyph)} — {letter.Name}"));
        }
    }
}
