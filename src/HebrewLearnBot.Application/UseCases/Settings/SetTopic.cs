using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;

namespace HebrewLearnBot.Application.UseCases.Settings;

public sealed class SetTopic
{
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly IClock _clock;

    public SetTopic(IUserSettingsRepository userSettingsRepository, ITopicRepository topicRepository, IClock clock)
    {
        _userSettingsRepository = userSettingsRepository;
        _topicRepository = topicRepository;
        _clock = clock;
    }

    public async Task ExecuteAsync(SetTopicCommand command, CancellationToken cancellationToken)
    {
        if (command.TopicId is { } topicId)
        {
            var topic = await _topicRepository.GetByIdAsync(topicId, cancellationToken);

            if (topic is null)
            {
                throw new InvalidOperationException($"Topic with id {topicId} not found.");
            }
        }

        var now = _clock.UtcNow;
        var settings = await _userSettingsRepository.GetOrCreateAsync(command.UserId, now, cancellationToken);

        settings.SelectTopic(command.TopicId, now);

        await _userSettingsRepository.UpdateAsync(settings, cancellationToken);
    }
}

public sealed record SetTopicCommand(long UserId, int? TopicId);
