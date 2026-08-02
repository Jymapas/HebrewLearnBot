using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Tests.Fakes;

public sealed class FakeLetterProgressRepository : ILetterProgressRepository
{
    public List<(long UserId, int LetterId, bool IsCorrect, DateTime AtUtc)> RegisteredAnswers { get; } = [];

    public List<(long UserId, int LetterId, DateTime AtUtc)> RegisteredSkips { get; } = [];

    public LetterStatistics StatisticsToReturn { get; set; } =
        new LetterStatistics(0, 0, 0, 0, 0, []);

    public int GetStatisticsCallCount { get; private set; }

    public long? LastStatisticsUserId { get; private set; }

    public int? LastTopProblemLetterCount { get; private set; }

    public Task RegisterAnswerAsync(long userId, int letterId, bool isCorrect, DateTime nowUtc, CancellationToken cancellationToken)
    {
        RegisteredAnswers.Add((userId, letterId, isCorrect, nowUtc));
        return Task.CompletedTask;
    }

    public Task RegisterSkipAsync(long userId, int letterId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        RegisteredSkips.Add((userId, letterId, nowUtc));
        return Task.CompletedTask;
    }

    public Task<LetterStatistics> GetStatisticsAsync(long userId, int topProblemLetterCount, CancellationToken cancellationToken)
    {
        GetStatisticsCallCount++;
        LastStatisticsUserId = userId;
        LastTopProblemLetterCount = topProblemLetterCount;

        return Task.FromResult(StatisticsToReturn);
    }
}
