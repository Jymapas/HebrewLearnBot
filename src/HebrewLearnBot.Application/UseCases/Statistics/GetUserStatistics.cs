using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.UseCases.Statistics;

public sealed class GetUserStatistics
{
    private const int TopProblemWordCount = 5;

    private readonly IWordProgressRepository _wordProgressRepository;

    public GetUserStatistics(IWordProgressRepository wordProgressRepository)
    {
        _wordProgressRepository = wordProgressRepository;
    }

    public Task<UserStatistics> ExecuteAsync(long userId, CancellationToken cancellationToken)
    {
        return _wordProgressRepository.GetStatisticsAsync(userId, TopProblemWordCount, cancellationToken);
    }
}
