using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HebrewLearnBot.Infrastructure.Persistence.Repositories;

public sealed class WordProgressRepository : IWordProgressRepository
{
    private const int LearnedCorrectThreshold = 5;
    private const int LearnedStreakThreshold = 3;

    private readonly HebrewDbContext _dbContext;

    public WordProgressRepository(HebrewDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RegisterAnswerAsync(long userId, int wordId, bool isCorrect, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var progress = await _dbContext.WordProgress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.WordId == wordId, cancellationToken);

        if (progress is null)
        {
            progress = new WordProgress(userId, wordId, nowUtc);
            _dbContext.WordProgress.Add(progress);
        }

        progress.RegisterAnswer(isCorrect, nowUtc);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RegisterSkipAsync(long userId, int wordId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var progress = await _dbContext.WordProgress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.WordId == wordId, cancellationToken);

        if (progress is null)
        {
            progress = new WordProgress(userId, wordId, nowUtc);
            _dbContext.WordProgress.Add(progress);
        }

        progress.RegisterSkip(nowUtc);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserStatistics> GetStatisticsAsync(long userId, int topProblemWordCount, CancellationToken cancellationToken)
    {
        var totalWords = await _dbContext.Words
            .AsNoTracking()
            .CountAsync(word => word.IsActive, cancellationToken);

        var seenWords = await _dbContext.WordProgress
            .AsNoTracking()
            .CountAsync(p => p.UserId == userId, cancellationToken);

        var learnedWords = await _dbContext.WordProgress
            .AsNoTracking()
            .CountAsync(p => p.UserId == userId && p.CorrectCount >= LearnedCorrectThreshold && p.Streak >= LearnedStreakThreshold, cancellationToken);

        var totalAnswers = await _dbContext.WordProgress
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .SumAsync(p => p.CorrectCount + p.WrongCount, cancellationToken);

        var correctAnswers = await _dbContext.WordProgress
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .SumAsync(p => p.CorrectCount, cancellationToken);

        var topProblemWords = await (
                from progress in _dbContext.WordProgress.AsNoTracking()
                where progress.UserId == userId && progress.WrongCount > 0
                join word in _dbContext.Words.AsNoTracking() on progress.WordId equals word.Id
                join topic in _dbContext.Topics.AsNoTracking() on word.TopicId equals topic.Id
                orderby progress.WrongCount descending
                select new WordBrief(
                    word.Id,
                    word.Hebrew,
                    word.HebrewVocalized,
                    word.Transliteration,
                    word.TranslationRu,
                    word.Gender,
                    topic.Name))
            .Take(topProblemWordCount)
            .ToListAsync(cancellationToken);

        return new UserStatistics(totalWords, seenWords, learnedWords, totalAnswers, correctAnswers, topProblemWords);
    }
}
