using HebrewLearnBot.Domain.Entities;

namespace HebrewLearnBot.Application.Abstractions.Persistence;

public interface IUserSettingsRepository
{
    Task<UserSettings> GetOrCreateAsync(long userId, DateTime nowUtc, CancellationToken cancellationToken);

    Task UpdateAsync(UserSettings settings, CancellationToken cancellationToken);
}
