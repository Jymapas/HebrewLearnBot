using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;

namespace HebrewLearnBot.Application.UseCases.Alphabet;

public sealed class SkipLetterQuestion
{
    private readonly ILetterProgressRepository _letterProgressRepository;
    private readonly IClock _clock;

    public SkipLetterQuestion(ILetterProgressRepository letterProgressRepository, IClock clock)
    {
        _letterProgressRepository = letterProgressRepository;
        _clock = clock;
    }

    public async Task ExecuteAsync(SkipLetterQuestionCommand command, CancellationToken cancellationToken)
    {
        await _letterProgressRepository.RegisterSkipAsync(
            command.UserId,
            command.LetterId,
            _clock.UtcNow,
            cancellationToken);
    }
}

public sealed record SkipLetterQuestionCommand(long UserId, int LetterId);
