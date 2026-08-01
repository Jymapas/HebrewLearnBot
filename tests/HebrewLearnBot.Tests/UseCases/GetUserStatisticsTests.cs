using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.UseCases.Statistics;
using HebrewLearnBot.Tests.Fakes;

namespace HebrewLearnBot.Tests.UseCases;

public sealed class GetUserStatisticsTests
{
    [Test]
    public async Task ExecuteAsync_DelegatesToRepositoryWithTopWordLimit()
    {
        var repository = new FakeWordProgressRepository
        {
            StatisticsToReturn = new UserStatistics(100, 50, 20, 200, 150, [])
        };
        var useCase = new GetUserStatistics(repository);

        var result = await useCase.ExecuteAsync(42, CancellationToken.None);

        Assert.That(result.TotalWords, Is.EqualTo(100));
        Assert.That(repository.GetStatisticsCallCount, Is.EqualTo(1));
        Assert.That(repository.LastStatisticsUserId, Is.EqualTo(42));
        Assert.That(repository.LastTopProblemWordCount, Is.EqualTo(5));
    }
}
