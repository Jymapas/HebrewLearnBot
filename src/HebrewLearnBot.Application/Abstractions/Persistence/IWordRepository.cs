using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Abstractions.Persistence;

public interface IWordRepository
{
    Task<int> CountActiveAsync(int? topicId, CancellationToken cancellationToken);

    Task<IReadOnlyList<WordBrief>> GetBriefsByIdsAsync(IReadOnlyCollection<int> wordIds, CancellationToken cancellationToken);
}
