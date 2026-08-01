using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Abstractions.Persistence;

public interface IQuizCandidateReader
{
    Task<IReadOnlyList<QuizCandidate>> LoadCandidatesAsync(long userId, int? topicId, int limit, CancellationToken cancellationToken);
}
