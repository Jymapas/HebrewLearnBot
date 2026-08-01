using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HebrewLearnBot.Infrastructure.Persistence.Repositories;

public sealed class WordRepository : IWordRepository
{
    private readonly HebrewDbContext _dbContext;

    public WordRepository(HebrewDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CountActiveAsync(int? topicId, CancellationToken cancellationToken)
    {
        return await _dbContext.Words
            .CountAsync(word => word.IsActive && (topicId == null || word.TopicId == topicId), cancellationToken);
    }

    public async Task<IReadOnlyList<WordBrief>> GetBriefsByIdsAsync(IReadOnlyCollection<int> wordIds, CancellationToken cancellationToken)
    {
        if (wordIds.Count == 0)
        {
            return Array.Empty<WordBrief>();
        }

        var briefs = await _dbContext.Words
            .AsNoTracking()
            .Where(word => wordIds.Contains(word.Id))
            .Join(
                _dbContext.Topics.AsNoTracking(),
                word => word.TopicId,
                topic => topic.Id,
                (word, topic) => new WordBrief(
                    word.Id,
                    word.Hebrew,
                    word.HebrewVocalized,
                    word.Transliteration,
                    word.TranslationRu,
                    word.Gender,
                    topic.Name))
            .ToListAsync(cancellationToken);

        var order = new Dictionary<int, int>();
        var index = 0;
        foreach (var wordId in wordIds)
        {
            order.TryAdd(wordId, index);
            index++;
        }

        return briefs
            .OrderBy(brief => order[brief.WordId])
            .ToList();
    }
}
