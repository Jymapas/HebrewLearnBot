using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Tests.Fakes;

public sealed class FakeWordProgressRepository : IWordProgressRepository
{
    public List<(long UserId, int WordId, bool IsCorrect, DateTime AtUtc)> RegisteredAnswers { get; } = [];

    public List<(long UserId, int WordId, DateTime AtUtc)> RegisteredSkips { get; } = [];

    public UserStatistics StatisticsToReturn { get; set; } =
        new UserStatistics(0, 0, 0, 0, 0, []);

    public int GetStatisticsCallCount { get; private set; }

    public long? LastStatisticsUserId { get; private set; }

    public int? LastTopProblemWordCount { get; private set; }

    public Task RegisterAnswerAsync(long userId, int wordId, bool isCorrect, DateTime nowUtc, CancellationToken cancellationToken)
    {
        RegisteredAnswers.Add((userId, wordId, isCorrect, nowUtc));
        return Task.CompletedTask;
    }

    public Task RegisterSkipAsync(long userId, int wordId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        RegisteredSkips.Add((userId, wordId, nowUtc));
        return Task.CompletedTask;
    }

    public Task<UserStatistics> GetStatisticsAsync(long userId, int topProblemWordCount, CancellationToken cancellationToken)
    {
        GetStatisticsCallCount++;
        LastStatisticsUserId = userId;
        LastTopProblemWordCount = topProblemWordCount;

        return Task.FromResult(StatisticsToReturn);
    }
}
