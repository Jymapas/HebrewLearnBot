using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Formatting;

namespace HebrewLearnBot.Tests.Formatting;

public sealed class LetterQuestionFormatterTests
{
    [Test]
    public void BuildOptionLabel_ReturnsLetterName()
    {
        var candidate = CreateCandidate(name: "Бет");

        var label = LetterQuestionFormatter.BuildOptionLabel(candidate);

        Assert.That(label, Is.EqualTo("Бет"));
    }

    [Test]
    public void BuildQuestionText_ContainsGlyphAndPrompt()
    {
        var candidate = CreateCandidate(glyph: "א");

        var text = LetterQuestionFormatter.BuildQuestionText(candidate, askedCount: 2, correctCount: 1);

        Assert.That(text, Does.Contain("Какая это буква?"));
        Assert.That(text, Does.Contain(candidate.Glyph));
    }

    [Test]
    public void BuildQuestionText_FirstQuestion_ShowsCounterOne()
    {
        var candidate = CreateCandidate();

        var text = LetterQuestionFormatter.BuildQuestionText(candidate, askedCount: 0, correctCount: 0);

        Assert.That(text, Does.Contain("Вопрос 1 · верно 0"));
    }

    private static LetterCandidate CreateCandidate(
        int letterId = 1,
        string glyph = "א",
        string name = "Алеф",
        bool isFinal = false)
    {
        return new LetterCandidate(
            letterId,
            glyph,
            name,
            isFinal,
            HasProgress: false,
            CorrectCount: 0,
            WrongCount: 0,
            Streak: 0,
            LastShownAtUtc: null,
            LastAnswerCorrect: null);
    }
}
