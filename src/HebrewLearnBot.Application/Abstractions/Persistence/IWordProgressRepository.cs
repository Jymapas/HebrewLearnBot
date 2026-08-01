using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Abstractions.Persistence;

public interface IWordProgressRepository
{
    Task RegisterAnswerAsync(long userId, int wordId, bool isCorrect, DateTime nowUtc, CancellationToken cancellationToken);

    Task RegisterSkipAsync(long userId, int wordId, DateTime nowUtc, CancellationToken cancellationToken);

    Task<UserStatistics> GetStatisticsAsync(long userId, int topProblemWordCount, CancellationToken cancellationToken);
}
