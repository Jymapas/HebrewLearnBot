using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Application.UseCases.Settings;

public sealed class SetQuizMode
{
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IClock _clock;

    public SetQuizMode(IUserSettingsRepository userSettingsRepository, IClock clock)
    {
        _userSettingsRepository = userSettingsRepository;
        _clock = clock;
    }

    public async Task ExecuteAsync(SetQuizModeCommand command, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var settings = await _userSettingsRepository.GetOrCreateAsync(command.UserId, now, cancellationToken);

        settings.SelectMode(command.Mode, now);

        await _userSettingsRepository.UpdateAsync(settings, cancellationToken);
    }
}

public sealed record SetQuizModeCommand(long UserId, QuizMode Mode);
