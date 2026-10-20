using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HebrewLearnBot.Infrastructure.Persistence.Repositories;

public sealed class NumberProgressRepository : INumberProgressRepository
{
    private readonly HebrewDbContext _dbContext;

    public NumberProgressRepository(HebrewDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<NumberProgress>> LoadAsync(long userId, CancellationToken cancellationToken)
    {
        return await _dbContext.NumberProgress
            .AsNoTracking()
            .Where(progress => progress.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task RegisterAnswerAsync(
        long userId,
        int value,
        NumberQuizMode mode,
        bool isCorrect,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var progress = await GetOrCreateAsync(userId, value, mode, nowUtc, cancellationToken);
        progress.RegisterAnswer(isCorrect, nowUtc);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RegisterSkipAsync(
        long userId,
        int value,
        NumberQuizMode mode,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var progress = await GetOrCreateAsync(userId, value, mode, nowUtc, cancellationToken);
        progress.RegisterSkip(nowUtc);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<NumberProgress> GetOrCreateAsync(
        long userId,
        int value,
        NumberQuizMode mode,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var progress = await _dbContext.NumberProgress
            .FirstOrDefaultAsync(
                item => item.UserId == userId && item.Value == value && item.Mode == mode,
                cancellationToken);

        if (progress is null)
        {
            progress = new NumberProgress(userId, value, mode, nowUtc);
            _dbContext.NumberProgress.Add(progress);
        }

        return progress;
    }
}
