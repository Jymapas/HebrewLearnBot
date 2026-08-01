using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Formatting;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Tests.Formatting;

public sealed class SummaryFormatterTests
{
    [Test]
    public void Build_NoMistakes_ShowsCongratulationWithoutWordList()
    {
        var summary = new QuizSummary(AskedCount: 5, CorrectCount: 5, WrongWords: Array.Empty<WordBrief>());

        var result = SummaryFormatter.Build(summary);

        Assert.That(result, Does.Contain("Ни одной ошибки, отлично! 🎉"));
        Assert.That(result, Does.Not.Contain("Стоит повторить:"));
    }

    [Test]
    public void Build_WithMistakes_ListsEachWrongWordOnce()
    {
        var word = CreateWordBrief();
        var summary = new QuizSummary(AskedCount: 4, CorrectCount: 2, WrongWords: new[] { word, word });

        var result = SummaryFormatter.Build(summary);

        Assert.That(result, Does.Contain("Стоит повторить:"));
        Assert.That(CountOccurrences(result, word.Hebrew), Is.EqualTo(1));
    }

    [Test]
    public void Build_ZeroAsked_ShowsNothingAnsweredMessage()
    {
        var summary = new QuizSummary(AskedCount: 0, CorrectCount: 0, WrongWords: Array.Empty<WordBrief>());

        var result = SummaryFormatter.Build(summary);

        Assert.That(result, Does.Contain("Вы не ответили ни на один вопрос."));
        Assert.That(result, Does.Not.Contain("Отвечено:"));
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
}
