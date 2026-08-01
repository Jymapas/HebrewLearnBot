using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.UseCases.Quiz;
using HebrewLearnBot.Tests.Fakes;

namespace HebrewLearnBot.Tests.UseCases;

public sealed class BuildQuizSummaryTests
{
    [Test]
    public async Task ExecuteAsync_NoWrongWords_ReturnsEmptyListWithoutHittingRepository()
    {
        var repository = new FakeWordRepository([]);
        var useCase = new BuildQuizSummary(repository);
        var command = new BuildQuizSummaryCommand(10, 8, Array.Empty<int>());

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result.AskedCount, Is.EqualTo(10));
        Assert.That(result.CorrectCount, Is.EqualTo(8));
        Assert.That(result.WrongWords, Is.Empty);
        Assert.That(repository.GetBriefsByIdsCallCount, Is.EqualTo(0));
    }

    [Test]
    public async Task ExecuteAsync_WithWrongWords_ReturnsBriefs()
    {
        var words = new List<WordBrief>
        {
            new(1, "שלום", null, "shalom", "привет", null, "Общая лексика"),
            new(2, "בית", null, "bayit", "дом", null, "Общая лексика")
        };
        var repository = new FakeWordRepository(words);
        var useCase = new BuildQuizSummary(repository);
        var command = new BuildQuizSummaryCommand(5, 3, new[] { 1, 2 });

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result.WrongWords, Has.Count.EqualTo(2));
        Assert.That(result.WrongWords[0].WordId, Is.EqualTo(1));
        Assert.That(result.WrongWords[1].WordId, Is.EqualTo(2));
        Assert.That(repository.GetBriefsByIdsCallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task ExecuteAsync_DuplicateWrongWordIds_AreDeduplicated()
    {
        var words = new List<WordBrief>
        {
            new(1, "שלום", null, "shalom", "привет", null, "Общая лексика"),
            new(2, "בית", null, "bayit", "дом", null, "Общая лексика")
        };
        var repository = new FakeWordRepository(words);
        var useCase = new BuildQuizSummary(repository);
        var command = new BuildQuizSummaryCommand(5, 3, new[] { 1, 2, 1, 2, 1 });

        var result = await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(result.WrongWords, Has.Count.EqualTo(2));
        Assert.That(result.WrongWords.Select(word => word.WordId), Is.EqualTo(new[] { 1, 2 }));
    }
}
