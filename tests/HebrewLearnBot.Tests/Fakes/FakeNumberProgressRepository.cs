using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Tests.Fakes;

public sealed class FakeNumberProgressRepository : INumberProgressRepository
{
    public List<NumberProgress> Progress { get; } = [];

    public Task<IReadOnlyList<NumberProgress>> LoadAsync(long userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<NumberProgress>>(Progress.Where(item => item.UserId == userId).ToList());
    }

    public Task RegisterAnswerAsync(long userId, int value, NumberQuizMode mode, bool isCorrect, DateTime nowUtc, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        FindOrCreate(userId, value, mode, nowUtc).RegisterAnswer(isCorrect, nowUtc);
        return Task.CompletedTask;
    }

    public Task RegisterSkipAsync(long userId, int value, NumberQuizMode mode, DateTime nowUtc, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        FindOrCreate(userId, value, mode, nowUtc).RegisterSkip(nowUtc);
        return Task.CompletedTask;
    }

    private NumberProgress FindOrCreate(long userId, int value, NumberQuizMode mode, DateTime nowUtc)
    {
        var progress = Progress.SingleOrDefault(item => item.UserId == userId && item.Value == value && item.Mode == mode);
        if (progress is null)
        {
            progress = new NumberProgress(userId, value, mode, nowUtc);
            Progress.Add(progress);
        }

        return progress;
    }
}
