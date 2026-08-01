using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Formatting;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Tests.Formatting;

public sealed class QuestionFormatterTests
{
    [Test]
    public void BuildOptionLabel_RuToHeWithTranslit_ContainsHebrewAndTransliteration()
    {
        var candidate = CreateCandidate();

        var label = QuestionFormatter.BuildOptionLabel(QuizMode.RuToHeWithTranslit, candidate);

        Assert.That(label, Does.Contain(candidate.Hebrew));
        Assert.That(label, Does.Contain(candidate.Transliteration));
    }

    [Test]
    public void BuildOptionLabel_RuToHe_DoesNotContainTransliteration()
    {
        var candidate = CreateCandidate();

        var label = QuestionFormatter.BuildOptionLabel(QuizMode.RuToHe, candidate);

        Assert.That(label, Does.Contain(candidate.Hebrew));
        Assert.That(label, Does.Not.Contain(candidate.Transliteration));
    }

    [Test]
    public void BuildOptionLabel_HeToRu_ReturnsRussianTranslation()
    {
        var candidate = CreateCandidate();

        var label = QuestionFormatter.BuildOptionLabel(QuizMode.HeToRu, candidate);

        Assert.That(label, Is.EqualTo(candidate.TranslationRu));
    }

    [Test]
    public void BuildQuestionText_TranslitToRu_ShowsTransliterationAndNoHebrew()
    {
        var candidate = CreateCandidate();

        var text = QuestionFormatter.BuildQuestionText(QuizMode.TranslitToRu, candidate, askedCount: 2, correctCount: 1);

        Assert.That(text, Does.Contain(candidate.Transliteration));
        Assert.That(text, Does.Not.Contain(candidate.Hebrew));
    }

    [Test]
    public void BuildQuestionText_FirstQuestion_ShowsCounterOne()
    {
        var candidate = CreateCandidate();

        var text = QuestionFormatter.BuildQuestionText(QuizMode.HeToRu, candidate, askedCount: 0, correctCount: 0);

        Assert.That(text, Does.Contain("Вопрос 1 · верно 0"));
    }

    private static QuizCandidate CreateCandidate(
        int wordId = 1,
        string hebrew = "שולחן",
        string transliteration = "shulkhan",
        string translationRu = "стол",
        Gender? gender = Gender.Masculine)
    {
        return new QuizCandidate(
            wordId,
            TopicId: 1,
            TopicName: "Дом",
            PartOfSpeech: PartOfSpeech.Noun,
            Gender: gender,
            Hebrew: hebrew,
            HebrewNormalized: hebrew,
            HebrewVocalized: null,
            Transliteration: transliteration,
            TranslationRu: translationRu,
            TranslationNormalized: translationRu,
            HasProgress: false,
            CorrectCount: 0,
            WrongCount: 0,
            Streak: 0,
            LastShownAtUtc: null,
            LastAnswerCorrect: null);
    }
}
