using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Formatting;

namespace HebrewLearnBot.Tests.Formatting;

public sealed class LetterReviewFormatterTests
{
    [Test]
    public void Build_CorrectAnswer_StartsWithSuccessMarker()
    {
        var letter = CreateLetterBrief();

        var result = LetterReviewFormatter.Build(letter, isCorrect: true, chosenLabel: null, askedCount: 1, correctCount: 1);

        Assert.That(result, Does.StartWith($"{BidiText.LeftToRightMark}✅"));
    }

    [Test]
    public void Build_WrongAnswer_MentionsChosenOption()
    {
        var letter = CreateLetterBrief();

        var result = LetterReviewFormatter.Build(letter, isCorrect: false, chosenLabel: "Бет", askedCount: 4, correctCount: 1);

        Assert.That(result, Does.Contain("Вы выбрали: Бет"));
    }

    [Test]
    public void Build_ContainsGlyphAndName()
    {
        var letter = CreateLetterBrief(glyph: "א", name: "Алеф");

        var result = LetterReviewFormatter.Build(letter, isCorrect: true, chosenLabel: null, askedCount: 1, correctCount: 1);

        Assert.That(result, Does.Contain("א"));
        Assert.That(result, Does.Contain("Алеф"));
    }

    [Test]
    public void Build_Skipped_NoChosenLabel_DoesNotMentionChoice()
    {
        var letter = CreateLetterBrief();

        var result = LetterReviewFormatter.Build(letter, isCorrect: false, chosenLabel: null, askedCount: 1, correctCount: 0);

        Assert.That(result, Does.Not.Contain("Вы выбрали:"));
    }

    private static LetterBrief CreateLetterBrief(int letterId = 1, string glyph = "א", string name = "Алеф", bool isFinal = false)
    {
        return new LetterBrief(letterId, glyph, name, isFinal);
    }
}
