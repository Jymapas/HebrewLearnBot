using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Abstractions.Persistence;

public interface ILetterCandidateReader
{
    Task<IReadOnlyList<LetterCandidate>> LoadCandidatesAsync(long userId, CancellationToken cancellationToken);
}
