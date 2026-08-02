using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.UseCases.Alphabet;
using HebrewLearnBot.Tests.Fakes;

namespace HebrewLearnBot.Tests.UseCases;

public sealed class GetLetterStatisticsTests
{
    [Test]
    public async Task ExecuteAsync_DelegatesToRepositoryWithTopLetterLimit()
    {
        var repository = new FakeLetterProgressRepository
        {
            StatisticsToReturn = new LetterStatistics(31, 20, 5, 100, 80, [])
        };
        var useCase = new GetLetterStatistics(repository);

        var result = await useCase.ExecuteAsync(42, CancellationToken.None);

        Assert.That(result.TotalLetters, Is.EqualTo(31));
        Assert.That(repository.GetStatisticsCallCount, Is.EqualTo(1));
        Assert.That(repository.LastStatisticsUserId, Is.EqualTo(42));
        Assert.That(repository.LastTopProblemLetterCount, Is.EqualTo(5));
    }
}
