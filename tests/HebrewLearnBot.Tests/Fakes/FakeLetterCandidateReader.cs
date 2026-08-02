using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Tests.Fakes;

public sealed class FakeLetterCandidateReader : ILetterCandidateReader
{
    private readonly List<LetterCandidate> _candidates;

    public FakeLetterCandidateReader(IEnumerable<LetterCandidate> candidates)
    {
        _candidates = candidates.ToList();
    }

    public int CallCount { get; private set; }

    public long? LastUserId { get; private set; }

    public Task<IReadOnlyList<LetterCandidate>> LoadCandidatesAsync(long userId, CancellationToken cancellationToken)
    {
        CallCount++;
        LastUserId = userId;

        return Task.FromResult<IReadOnlyList<LetterCandidate>>(_candidates.ToList());
    }
}
