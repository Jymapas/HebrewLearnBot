using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HebrewLearnBot.Infrastructure.Persistence.Repositories;

public sealed class TopicRepository : ITopicRepository
{
    private readonly HebrewDbContext _dbContext;

    public TopicRepository(HebrewDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TopicListItem>> ListWithWordCountsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Topics
            .AsNoTracking()
            .OrderBy(topic => topic.Name)
            .Select(topic => new TopicListItem(
                topic.Id,
                topic.Name,
                _dbContext.Words.Count(word => word.TopicId == topic.Id && word.IsActive)))
            .ToListAsync(cancellationToken);
    }

    public async Task<TopicListItem?> GetByIdAsync(int topicId, CancellationToken cancellationToken)
    {
        return await _dbContext.Topics
            .AsNoTracking()
            .Where(topic => topic.Id == topicId)
            .Select(topic => new TopicListItem(
                topic.Id,
                topic.Name,
                _dbContext.Words.Count(word => word.TopicId == topic.Id && word.IsActive)))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
