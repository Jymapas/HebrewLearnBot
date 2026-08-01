using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HebrewLearnBot.Infrastructure.Persistence.Readers;

public sealed class QuizCandidateReader : IQuizCandidateReader
{
    private readonly HebrewDbContext _dbContext;

    public QuizCandidateReader(HebrewDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<QuizCandidate>> LoadCandidatesAsync(long userId, int? topicId, int limit, CancellationToken cancellationToken)
    {
        // Сортировка и Take выполняются до финальной проекции в QuizCandidate, чтобы EF Core
        // транслировал ORDER BY / LIMIT напрямую по столбцам join'а, а не пытался
        // сопоставлять свойства уже сконструированной записи с исходным выражением.
        var query =
            from word in _dbContext.Words.AsNoTracking()
            join topic in _dbContext.Topics.AsNoTracking() on word.TopicId equals topic.Id
            join progress in _dbContext.WordProgress.AsNoTracking()
                on new { WordId = word.Id, UserId = userId } equals new { progress.WordId, progress.UserId } into progressGroup
            from progress in progressGroup.DefaultIfEmpty()
            where word.IsActive && (topicId == null || word.TopicId == topicId)
            orderby progress == null ? 0 : 1, progress == null ? DateTime.MinValue : (progress.LastShownAtUtc ?? DateTime.MinValue)
            select new QuizCandidate(
                word.Id,
                word.TopicId,
                topic.Name,
                word.PartOfSpeech,
                word.Gender,
                word.Hebrew,
                word.HebrewNormalized,
                word.HebrewVocalized,
                word.Transliteration,
                word.TranslationRu,
                word.TranslationNormalized,
                progress != null,
                progress == null ? 0 : progress.CorrectCount,
                progress == null ? 0 : progress.WrongCount,
                progress == null ? 0 : progress.Streak,
                progress == null ? (DateTime?)null : progress.LastShownAtUtc,
                progress == null ? (bool?)null : progress.LastAnswerCorrect);

        return await query
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
