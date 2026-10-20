using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Domain.Numbers;

namespace HebrewLearnBot.Application.UseCases.Numbers;

public sealed class GetNumberStatistics
{
    private readonly INumberProgressRepository _repository;

    public GetNumberStatistics(INumberProgressRepository repository)
    {
        _repository = repository;
    }

    public async Task<NumberStatistics> ExecuteAsync(long userId, CancellationToken cancellationToken)
    {
        var progress = (await _repository.LoadAsync(userId, cancellationToken))
            .Where(item => item.UserId == userId).ToList();
        return new NumberStatistics(
            HebrewNumbers.All.Count,
            progress.Where(item => item.LastShownAtUtc.HasValue).Select(item => item.Value).Distinct().Count(),
            progress.Where(item => item.IsLearned).Select(item => item.Value).Distinct().Count(),
            progress.Sum(item => item.CorrectCount + item.WrongCount),
            progress.Sum(item => item.CorrectCount));
    }
}
