using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Application.Abstractions.Persistence;

public interface INumberProgressRepository
{
    Task<IReadOnlyList<NumberProgress>> LoadAsync(long userId, CancellationToken cancellationToken);

    Task RegisterAnswerAsync(long userId, int value, NumberQuizMode mode, bool isCorrect, DateTime nowUtc, CancellationToken cancellationToken);

    Task RegisterSkipAsync(long userId, int value, NumberQuizMode mode, DateTime nowUtc, CancellationToken cancellationToken);
}
