using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.UseCases.Topics;

public sealed class ListTopics
{
    private readonly ITopicRepository _topicRepository;

    public ListTopics(ITopicRepository topicRepository)
    {
        _topicRepository = topicRepository;
    }

    public Task<IReadOnlyList<TopicListItem>> ExecuteAsync(CancellationToken cancellationToken)
    {
        return _topicRepository.ListWithWordCountsAsync(cancellationToken);
    }
}
