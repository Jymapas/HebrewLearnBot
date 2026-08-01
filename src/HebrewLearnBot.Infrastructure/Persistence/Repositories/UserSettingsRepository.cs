using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HebrewLearnBot.Infrastructure.Persistence.Repositories;

public sealed class UserSettingsRepository : IUserSettingsRepository
{
    private readonly HebrewDbContext _dbContext;

    public UserSettingsRepository(HebrewDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserSettings> GetOrCreateAsync(long userId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var settings = await _dbContext.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (settings is not null)
        {
            return settings;
        }

        settings = new UserSettings(userId, nowUtc);
        _dbContext.UserSettings.Add(settings);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return settings;
    }

    public async Task UpdateAsync(UserSettings settings, CancellationToken cancellationToken)
    {
        _dbContext.UserSettings.Update(settings);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
