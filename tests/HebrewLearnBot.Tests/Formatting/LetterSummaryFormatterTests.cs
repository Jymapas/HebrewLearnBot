using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Formatting;

namespace HebrewLearnBot.Tests.Formatting;

public sealed class LetterSummaryFormatterTests
{
    [Test]
    public void Build_NoMistakes_ShowsCongratulationWithoutLetterList()
    {
        var summary = new LetterQuizSummary(AskedCount: 5, CorrectCount: 5, WrongLetters: Array.Empty<LetterBrief>());

        var result = LetterSummaryFormatter.Build(summary);

        Assert.That(result, Does.Contain("Ни одной ошибки, отлично! 🎉"));
        Assert.That(result, Does.Not.Contain("Стоит повторить:"));
    }

    [Test]
    public void Build_WithMistakes_ListsEachWrongLetterOnce()
    {
        var letter = CreateLetterBrief();
        var summary = new LetterQuizSummary(AskedCount: 4, CorrectCount: 2, WrongLetters: new[] { letter, letter });

        var result = LetterSummaryFormatter.Build(summary);

        Assert.That(result, Does.Contain("Стоит повторить:"));
        Assert.That(CountOccurrences(result, letter.Name), Is.EqualTo(1));
    }

    [Test]
    public void Build_ZeroAsked_ShowsNothingAnsweredMessage()
    {
        var summary = new LetterQuizSummary(AskedCount: 0, CorrectCount: 0, WrongLetters: Array.Empty<LetterBrief>());

        var result = LetterSummaryFormatter.Build(summary);

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

    private static LetterBrief CreateLetterBrief(int letterId = 1, string glyph = "א", string name = "Алеф", bool isFinal = false)
    {
        return new LetterBrief(letterId, glyph, name, isFinal);
    }
}
