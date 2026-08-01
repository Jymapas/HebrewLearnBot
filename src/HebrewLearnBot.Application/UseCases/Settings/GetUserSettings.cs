using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Domain.Entities;

namespace HebrewLearnBot.Application.UseCases.Settings;

public sealed class GetUserSettings
{
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IClock _clock;

    public GetUserSettings(IUserSettingsRepository userSettingsRepository, IClock clock)
    {
        _userSettingsRepository = userSettingsRepository;
        _clock = clock;
    }

    public async Task<UserSettings> ExecuteAsync(long userId, CancellationToken cancellationToken)
    {
        return await _userSettingsRepository.GetOrCreateAsync(userId, _clock.UtcNow, cancellationToken);
    }
}
