using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;

namespace HebrewLearnBot.Application.UseCases.Quiz;

public sealed class SkipQuizQuestion
{
    private readonly IWordProgressRepository _wordProgressRepository;
    private readonly IClock _clock;

    public SkipQuizQuestion(IWordProgressRepository wordProgressRepository, IClock clock)
    {
        _wordProgressRepository = wordProgressRepository;
        _clock = clock;
    }

    public async Task ExecuteAsync(SkipQuizQuestionCommand command, CancellationToken cancellationToken)
    {
        await _wordProgressRepository.RegisterSkipAsync(
            command.UserId,
            command.WordId,
            _clock.UtcNow,
            cancellationToken);
    }
}

public sealed record SkipQuizQuestionCommand(long UserId, int WordId);
