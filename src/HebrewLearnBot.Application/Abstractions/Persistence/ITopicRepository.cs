using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Abstractions.Persistence;

public interface ITopicRepository
{
    Task<IReadOnlyList<TopicListItem>> ListWithWordCountsAsync(CancellationToken cancellationToken);

    Task<TopicListItem?> GetByIdAsync(int topicId, CancellationToken cancellationToken);
}
