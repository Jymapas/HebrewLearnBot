using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Domain.Alphabet;
using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HebrewLearnBot.Infrastructure.Persistence.Readers;

public sealed class LetterCandidateReader : ILetterCandidateReader
{
    private readonly HebrewDbContext _dbContext;

    public LetterCandidateReader(HebrewDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Букв нет в БД — это статический список в Domain. Прогресс подтягивается одним
    // запросом и сопоставляется с ним в памяти, а не через SQL join.
    public async Task<IReadOnlyList<LetterCandidate>> LoadCandidatesAsync(long userId, CancellationToken cancellationToken)
    {
        var progressByLetterId = await _dbContext.LetterProgress
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.LetterId, cancellationToken);

        var candidates = new List<LetterCandidate>(HebrewAlphabet.All.Count);

        foreach (var letter in HebrewAlphabet.All)
        {
            progressByLetterId.TryGetValue(letter.Id, out var progress);

            candidates.Add(new LetterCandidate(
                letter.Id,
                letter.Glyph,
                letter.Name,
                letter.IsFinal,
                progress is not null,
                progress?.CorrectCount ?? 0,
                progress?.WrongCount ?? 0,
                progress?.Streak ?? 0,
                progress?.LastShownAtUtc,
                progress?.LastAnswerCorrect));
        }

        return candidates;
    }
}
