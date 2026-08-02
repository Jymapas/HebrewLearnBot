using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.UseCases.Alphabet;

public sealed class GetLetterStatistics
{
    private const int TopProblemLetterCount = 5;

    private readonly ILetterProgressRepository _letterProgressRepository;

    public GetLetterStatistics(ILetterProgressRepository letterProgressRepository)
    {
        _letterProgressRepository = letterProgressRepository;
    }

    public Task<LetterStatistics> ExecuteAsync(long userId, CancellationToken cancellationToken)
    {
        return _letterProgressRepository.GetStatisticsAsync(userId, TopProblemLetterCount, cancellationToken);
    }
}
