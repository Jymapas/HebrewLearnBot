using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Domain.Entities;

namespace HebrewLearnBot.Tests.Fakes;

public sealed class FakeUserSettingsRepository : IUserSettingsRepository
{
    private readonly Dictionary<long, UserSettings> _settingsByUserId = [];

    public int UpdateCallCount { get; private set; }

    public void Seed(UserSettings settings)
    {
        _settingsByUserId[settings.UserId] = settings;
    }

    public Task<UserSettings> GetOrCreateAsync(long userId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        if (!_settingsByUserId.TryGetValue(userId, out var settings))
        {
            settings = new UserSettings(userId, nowUtc);
            _settingsByUserId[userId] = settings;
        }

        return Task.FromResult(settings);
    }

    public Task UpdateAsync(UserSettings settings, CancellationToken cancellationToken)
    {
        UpdateCallCount++;
        _settingsByUserId[settings.UserId] = settings;
        return Task.CompletedTask;
    }
}
