using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Application.UseCases.Numbers;

public sealed class SubmitNumberAnswer
{
    private readonly INumberProgressRepository _repository;
    private readonly IClock _clock;

    public SubmitNumberAnswer(INumberProgressRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public Task ExecuteAsync(long userId, int value, NumberQuizMode mode, bool isCorrect, CancellationToken cancellationToken)
    {
        return _repository.RegisterAnswerAsync(userId, value, mode, isCorrect, _clock.UtcNow, cancellationToken);
    }
}
