using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Formatting;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Tests.Formatting;

public sealed class ReviewFormatterTests
{
    [Test]
    public void Build_HeToRu_NeverContainsTransliteration()
    {
        var word = CreateWordBrief(transliteration: "shulkhan");

        var resultCorrect = ReviewFormatter.Build(QuizMode.HeToRu, word, isCorrect: true, chosenLabel: null, askedCount: 3, correctCount: 2);
        var resultWrong = ReviewFormatter.Build(QuizMode.HeToRu, word, isCorrect: false, chosenLabel: "стул", askedCount: 3, correctCount: 2);

        Assert.That(resultCorrect, Does.Not.Contain("shulkhan"));
        Assert.That(resultWrong, Does.Not.Contain("shulkhan"));
    }

    [Test]
    public void Build_RuToHe_RevealsTransliteration()
    {
        var word = CreateWordBrief(transliteration: "shulkhan");

        var result = ReviewFormatter.Build(QuizMode.RuToHe, word, isCorrect: true, chosenLabel: null, askedCount: 1, correctCount: 1);

        Assert.That(result, Does.Contain("shulkhan"));
    }

    [Test]
    public void Build_RuToHeWithTranslit_RevealsTransliteration()
    {
        var word = CreateWordBrief(transliteration: "shulkhan");

        var result = ReviewFormatter.Build(QuizMode.RuToHeWithTranslit, word, isCorrect: true, chosenLabel: null, askedCount: 1, correctCount: 1);

        Assert.That(result, Does.Contain("shulkhan"));
    }

    [Test]
    public void Build_WrongAnswer_MentionsChosenOption()
    {
        var word = CreateWordBrief();

        var result = ReviewFormatter.Build(QuizMode.HeToRu, word, isCorrect: false, chosenLabel: "стул", askedCount: 4, correctCount: 1);

        Assert.That(result, Does.Contain("Вы выбрали: стул"));
    }

    [Test]
    public void Build_CorrectAnswer_StartsWithSuccessMarker()
    {
        var word = CreateWordBrief();

        var result = ReviewFormatter.Build(QuizMode.HeToRu, word, isCorrect: true, chosenLabel: null, askedCount: 1, correctCount: 1);

        Assert.That(result, Does.StartWith($"{BidiText.LeftToRightMark}✅"));
    }

    [Test]
    public void Build_WordWithoutGender_OmitsGenderPart()
    {
        var word = CreateWordBrief(gender: null);

        var result = ReviewFormatter.Build(QuizMode.HeToRu, word, isCorrect: true, chosenLabel: null, askedCount: 1, correctCount: 1);

        Assert.That(result, Does.Not.Contain("Род:"));
    }

    [Test]
    public void Build_WordWithVocalized_ShowsVocalizedLine()
    {
        var word = CreateWordBrief(hebrewVocalized: "שֻׁלְחָן");

        var result = ReviewFormatter.Build(QuizMode.HeToRu, word, isCorrect: true, chosenLabel: null, askedCount: 1, correctCount: 1);

        Assert.That(result, Does.Contain("Огласовки:"));
        Assert.That(result, Does.Contain("שֻׁלְחָן"));
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
