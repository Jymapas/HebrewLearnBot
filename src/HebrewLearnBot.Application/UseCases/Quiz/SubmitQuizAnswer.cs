using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;

namespace HebrewLearnBot.Application.UseCases.Quiz;

public sealed class SubmitQuizAnswer
{
    private readonly IWordProgressRepository _wordProgressRepository;
    private readonly IClock _clock;

    public SubmitQuizAnswer(IWordProgressRepository wordProgressRepository, IClock clock)
    {
        _wordProgressRepository = wordProgressRepository;
        _clock = clock;
    }

    public async Task ExecuteAsync(SubmitQuizAnswerCommand command, CancellationToken cancellationToken)
    {
        await _wordProgressRepository.RegisterAnswerAsync(
            command.UserId,
            command.WordId,
            command.IsCorrect,
            _clock.UtcNow,
            cancellationToken);
    }
}

public sealed record SubmitQuizAnswerCommand(long UserId, int WordId, bool IsCorrect);
