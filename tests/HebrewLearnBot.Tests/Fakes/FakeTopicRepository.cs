using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Tests.Fakes;

public sealed class FakeTopicRepository : ITopicRepository
{
    private readonly List<TopicListItem> _topics;

    public FakeTopicRepository(IEnumerable<TopicListItem> topics)
    {
        _topics = topics.ToList();
    }

    public Task<IReadOnlyList<TopicListItem>> ListWithWordCountsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<TopicListItem>>(_topics);
    }

    public Task<TopicListItem?> GetByIdAsync(int topicId, CancellationToken cancellationToken)
    {
        var topic = _topics.FirstOrDefault(item => item.TopicId == topicId);
        return Task.FromResult(topic);
    }
}
