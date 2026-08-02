using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Abstractions.Persistence;

public interface ILetterProgressRepository
{
    Task RegisterAnswerAsync(long userId, int letterId, bool isCorrect, DateTime nowUtc, CancellationToken cancellationToken);

    Task RegisterSkipAsync(long userId, int letterId, DateTime nowUtc, CancellationToken cancellationToken);

    Task<LetterStatistics> GetStatisticsAsync(long userId, int topProblemLetterCount, CancellationToken cancellationToken);
}
