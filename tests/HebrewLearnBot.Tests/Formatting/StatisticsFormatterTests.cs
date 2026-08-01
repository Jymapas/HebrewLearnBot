using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Formatting;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Tests.Formatting;

public sealed class StatisticsFormatterTests
{
    [Test]
    public void Build_NoAnswers_ShowsZeroState()
    {
        var statistics = new UserStatistics(
            TotalWords: 10,
            SeenWords: 0,
            LearnedWords: 0,
            TotalAnswers: 0,
            CorrectAnswers: 0,
            TopProblemWords: Array.Empty<WordBrief>());

        var result = StatisticsFormatter.Build(statistics);

        Assert.That(result, Does.Contain("Точность: —"));
        Assert.That(result, Does.Contain("Вы ещё не отвечали на вопросы."));
    }

    [Test]
    public void Build_WithProblemWords_ListsThem()
    {
        var word = CreateWordBrief();
        var statistics = new UserStatistics(
            TotalWords: 10,
            SeenWords: 5,
            LearnedWords: 2,
            TotalAnswers: 20,
            CorrectAnswers: 15,
            TopProblemWords: new[] { word });

        var result = StatisticsFormatter.Build(statistics);

        Assert.That(result, Does.Contain("Чаще всего ошибаетесь:"));
        Assert.That(result, Does.Contain(word.TranslationRu));
        Assert.That(result, Does.Contain("Точность: 75%"));
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
