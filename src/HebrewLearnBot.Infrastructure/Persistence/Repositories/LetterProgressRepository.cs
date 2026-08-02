using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Domain.Alphabet;
using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HebrewLearnBot.Infrastructure.Persistence.Repositories;

public sealed class LetterProgressRepository : ILetterProgressRepository
{
    private const int LearnedCorrectThreshold = 5;
    private const int LearnedStreakThreshold = 3;

    private readonly HebrewDbContext _dbContext;

    public LetterProgressRepository(HebrewDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RegisterAnswerAsync(long userId, int letterId, bool isCorrect, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var progress = await _dbContext.LetterProgress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LetterId == letterId, cancellationToken);

        if (progress is null)
        {
            progress = new LetterProgress(userId, letterId, nowUtc);
            _dbContext.LetterProgress.Add(progress);
        }

        progress.RegisterAnswer(isCorrect, nowUtc);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RegisterSkipAsync(long userId, int letterId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var progress = await _dbContext.LetterProgress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LetterId == letterId, cancellationToken);

        if (progress is null)
        {
            progress = new LetterProgress(userId, letterId, nowUtc);
            _dbContext.LetterProgress.Add(progress);
        }

        progress.RegisterSkip(nowUtc);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<LetterStatistics> GetStatisticsAsync(long userId, int topProblemLetterCount, CancellationToken cancellationToken)
    {
        var totalLetters = HebrewAlphabet.All.Count;

        var seenLetters = await _dbContext.LetterProgress
            .AsNoTracking()
            .CountAsync(p => p.UserId == userId, cancellationToken);

        var learnedLetters = await _dbContext.LetterProgress
            .AsNoTracking()
            .CountAsync(p => p.UserId == userId && p.CorrectCount >= LearnedCorrectThreshold && p.Streak >= LearnedStreakThreshold, cancellationToken);

        var totalAnswers = await _dbContext.LetterProgress
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .SumAsync(p => p.CorrectCount + p.WrongCount, cancellationToken);

        var correctAnswers = await _dbContext.LetterProgress
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .SumAsync(p => p.CorrectCount, cancellationToken);

        var topProblemLetterIds = await _dbContext.LetterProgress
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.WrongCount > 0)
            .OrderByDescending(p => p.WrongCount)
            .Take(topProblemLetterCount)
            .Select(p => p.LetterId)
            .ToListAsync(cancellationToken);

        var topProblemLetters = topProblemLetterIds
            .Select(letterId => HebrewAlphabet.All.FirstOrDefault(letter => letter.Id == letterId))
            .Where(letter => letter is not null)
            .Select(letter => new LetterBrief(letter!.Id, letter.Glyph, letter.Name, letter.IsFinal))
            .ToList();

        return new LetterStatistics(totalLetters, seenLetters, learnedLetters, totalAnswers, correctAnswers, topProblemLetters);
    }
}
