using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Formatting;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Tests.Formatting;

public sealed class StatisticsFormatterTests
{
    [Test]
    public void Build_NoAnswers_ShowsZeroStateForBothSections()
    {
        var statistics = new UserStatistics(
            TotalWords: 10,
            SeenWords: 0,
            LearnedWords: 0,
            TotalAnswers: 0,
            CorrectAnswers: 0,
            TopProblemWords: Array.Empty<WordBrief>());
        var letterStatistics = new LetterStatistics(
            TotalLetters: 31,
            SeenLetters: 0,
            LearnedLetters: 0,
            TotalAnswers: 0,
            CorrectAnswers: 0,
            TopProblemLetters: Array.Empty<LetterBrief>());

        var result = StatisticsFormatter.Build(statistics, letterStatistics);

        Assert.That(result, Does.Contain("📖 Слова"));
        Assert.That(result, Does.Contain("🔤 Алфавит"));
        Assert.That(CountOccurrences(result, "Точность: —"), Is.EqualTo(2));
        Assert.That(CountOccurrences(result, "Вы ещё не отвечали на вопросы."), Is.EqualTo(2));
    }

    [Test]
    public void Build_WithProblemWordsAndLetters_ListsThem()
    {
        var word = CreateWordBrief();
        var letter = CreateLetterBrief();
        var statistics = new UserStatistics(
            TotalWords: 10,
            SeenWords: 5,
            LearnedWords: 2,
            TotalAnswers: 20,
            CorrectAnswers: 15,
            TopProblemWords: new[] { word });
        var letterStatistics = new LetterStatistics(
            TotalLetters: 31,
            SeenLetters: 10,
            LearnedLetters: 4,
            TotalAnswers: 8,
            CorrectAnswers: 6,
            TopProblemLetters: new[] { letter });

        var result = StatisticsFormatter.Build(statistics, letterStatistics);

        Assert.That(result, Does.Contain(word.TranslationRu));
        Assert.That(result, Does.Contain(letter.Name));
        Assert.That(CountOccurrences(result, "Точность: 75%"), Is.EqualTo(2));
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;

        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static WordBrief CreateWordBrief(
        int wordId = 1,
        string hebrew = "שולחן",
        string? hebrewVocalized = null,
        string transliteration = "shulkhan",
        string translationRu = "стол",
        Gender? gender = Gender.Masculine,
        string topicName = "Дом")
    {
        return new WordBrief(wordId, hebrew, hebrewVocalized, transliteration, translationRu, gender, topicName);
    }

    private static LetterBrief CreateLetterBrief(int letterId = 1, string glyph = "א", string name = "Алеф", bool isFinal = false)
    {
        return new LetterBrief(letterId, glyph, name, isFinal);
    }
}
