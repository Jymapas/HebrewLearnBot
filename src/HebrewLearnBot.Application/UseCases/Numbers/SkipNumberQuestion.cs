using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Application.UseCases.Numbers;

public sealed class SkipNumberQuestion
{
    private readonly INumberProgressRepository _repository;
    private readonly IClock _clock;

    public SkipNumberQuestion(INumberProgressRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public Task ExecuteAsync(long userId, int value, NumberQuizMode mode, CancellationToken cancellationToken)
    {
        return _repository.RegisterSkipAsync(userId, value, mode, _clock.UtcNow, cancellationToken);
    }
}
