using HebrewLearnBot.Application.UseCases.Alphabet;
using HebrewLearnBot.Domain.Alphabet;

namespace HebrewLearnBot.Tests.UseCases;

public sealed class BuildLetterQuizSummaryTests
{
    [Test]
    public void Execute_NoWrongLetters_ReturnsEmptyList()
    {
        var useCase = new BuildLetterQuizSummary();
        var command = new BuildLetterQuizSummaryCommand(10, 8, Array.Empty<int>());

        var result = useCase.Execute(command);

        Assert.That(result.AskedCount, Is.EqualTo(10));
        Assert.That(result.CorrectCount, Is.EqualTo(8));
        Assert.That(result.WrongLetters, Is.Empty);
    }

    [Test]
    public void Execute_WithWrongLetters_ReturnsBriefsFromAlphabet()
    {
        var useCase = new BuildLetterQuizSummary();
        var command = new BuildLetterQuizSummaryCommand(5, 3, new[] { 1, 2 });

        var result = useCase.Execute(command);

        Assert.That(result.WrongLetters, Has.Count.EqualTo(2));
        Assert.That(result.WrongLetters[0].LetterId, Is.EqualTo(1));
        Assert.That(result.WrongLetters[0].Name, Is.EqualTo(HebrewAlphabet.All.First(l => l.Id == 1).Name));
        Assert.That(result.WrongLetters[1].LetterId, Is.EqualTo(2));
    }

    [Test]
    public void Execute_DuplicateWrongLetterIds_AreDeduplicated()
    {
        var useCase = new BuildLetterQuizSummary();
        var command = new BuildLetterQuizSummaryCommand(5, 3, new[] { 1, 2, 1, 2, 1 });

        var result = useCase.Execute(command);

        Assert.That(result.WrongLetters, Has.Count.EqualTo(2));
        Assert.That(result.WrongLetters.Select(letter => letter.LetterId), Is.EqualTo(new[] { 1, 2 }));
    }
}
