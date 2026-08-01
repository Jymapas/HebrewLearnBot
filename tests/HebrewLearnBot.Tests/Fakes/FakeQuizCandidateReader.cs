using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Tests.Fakes;

public sealed class FakeQuizCandidateReader : IQuizCandidateReader
{
    private readonly List<QuizCandidate> _candidates;

    public FakeQuizCandidateReader(IEnumerable<QuizCandidate> candidates)
    {
        _candidates = candidates.ToList();
    }

    public int CallCount { get; private set; }

    public long? LastUserId { get; private set; }

    public int? LastTopicId { get; private set; }

    public int? LastLimit { get; private set; }

    public Task<IReadOnlyList<QuizCandidate>> LoadCandidatesAsync(long userId, int? topicId, int limit, CancellationToken cancellationToken)
    {
        CallCount++;
        LastUserId = userId;
        LastTopicId = topicId;
        LastLimit = limit;

        IEnumerable<QuizCandidate> filtered = _candidates;

        if (topicId is not null)
        {
            filtered = filtered.Where(candidate => candidate.TopicId == topicId.Value);
        }

        var result = filtered.Take(limit).ToList();

        return Task.FromResult<IReadOnlyList<QuizCandidate>>(result);
    }
}
