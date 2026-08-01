using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Tests.Fakes;

public sealed class FakeWordRepository : IWordRepository
{
    private readonly List<WordBrief> _words;

    public FakeWordRepository(IEnumerable<WordBrief> words)
    {
        _words = words.ToList();
    }

    public int CountActiveCallCount { get; private set; }

    public int GetBriefsByIdsCallCount { get; private set; }

    public int CountToReturn { get; set; }

    public Task<int> CountActiveAsync(int? topicId, CancellationToken cancellationToken)
    {
        CountActiveCallCount++;
        return Task.FromResult(CountToReturn);
    }

    public Task<IReadOnlyList<WordBrief>> GetBriefsByIdsAsync(IReadOnlyCollection<int> wordIds, CancellationToken cancellationToken)
    {
        GetBriefsByIdsCallCount++;

        var byId = _words.ToDictionary(word => word.WordId);
        var result = new List<WordBrief>();

        foreach (var wordId in wordIds)
        {
            if (byId.TryGetValue(wordId, out var brief))
            {
                result.Add(brief);
            }
        }

        return Task.FromResult<IReadOnlyList<WordBrief>>(result);
    }
}
