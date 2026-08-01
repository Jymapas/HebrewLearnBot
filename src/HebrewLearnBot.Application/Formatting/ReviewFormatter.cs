using System.Globalization;
using System.Text;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Application.Formatting;

public static class ReviewFormatter
{
    public static string Build(
        QuizMode mode,
        WordBrief correctWord,
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
        builder.Append(BuildBreakdown(mode, correctWord));

        if (!string.IsNullOrEmpty(correctWord.HebrewVocalized))
        {
            builder.Append('\n');
            builder.Append(BidiText.Line($"Огласовки: {BidiText.Isolate(correctWord.HebrewVocalized)}"));
        }

        builder.Append('\n');
        builder.Append(BidiText.Line(BuildTopicLine(correctWord)));

        builder.Append('\n');
        builder.Append('\n');
        builder.Append(BidiText.Line(
            $"📊 {correctCount.ToString(CultureInfo.InvariantCulture)} из {askedCount.ToString(CultureInfo.InvariantCulture)}"));

        return builder.ToString();
    }

    // Транскрипция здесь намеренно не появляется ни в одной ветке HeToRu.
    // Каждая физическая строка разбора уже обёрнута в BidiText.Line ровно один раз.
    private static string BuildBreakdown(QuizMode mode, WordBrief correctWord)
    {
        switch (mode)
        {
            case QuizMode.HeToRu:
                return BidiText.Line($"{BidiText.Isolate(correctWord.Hebrew)} — {correctWord.TranslationRu}");

            case QuizMode.TranslitToRu:
                return BidiText.Line($"{correctWord.Transliteration} — {correctWord.TranslationRu}")
                       + "\n" + BidiText.Line(BidiText.Isolate(correctWord.Hebrew));

            case QuizMode.RuToHeWithTranslit:
            case QuizMode.RuToHe:
                return BidiText.Line(
                    $"{correctWord.TranslationRu} — {BidiText.Isolate(correctWord.Hebrew)} ({correctWord.Transliteration})");

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    private static string BuildTopicLine(WordBrief correctWord)
    {
        var genderSuffix = correctWord.Gender switch
        {
            Gender.Masculine => " · Род: мужской",
            Gender.Feminine => " · Род: женский",
            _ => string.Empty
        };

        return $"Тема: {correctWord.TopicName}{genderSuffix}";
    }
}
