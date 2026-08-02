using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;

namespace HebrewLearnBot.Application.UseCases.Alphabet;

public sealed class SubmitLetterAnswer
{
    private readonly ILetterProgressRepository _letterProgressRepository;
    private readonly IClock _clock;

    public SubmitLetterAnswer(ILetterProgressRepository letterProgressRepository, IClock clock)
    {
        _letterProgressRepository = letterProgressRepository;
        _clock = clock;
    }

    public async Task ExecuteAsync(SubmitLetterAnswerCommand command, CancellationToken cancellationToken)
    {
        await _letterProgressRepository.RegisterAnswerAsync(
            command.UserId,
            command.LetterId,
            command.IsCorrect,
            _clock.UtcNow,
            cancellationToken);
    }
}

public sealed record SubmitLetterAnswerCommand(long UserId, int LetterId, bool IsCorrect);
