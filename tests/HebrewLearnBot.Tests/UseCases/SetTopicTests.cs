using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.UseCases.Settings;
using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Tests.Fakes;

namespace HebrewLearnBot.Tests.UseCases;

public sealed class SetTopicTests
{
    private static readonly DateTime NowUtc = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public void ExecuteAsync_UnknownTopic_ThrowsInvalidOperationException()
    {
        var settingsRepository = new FakeUserSettingsRepository();
        var topicRepository = new FakeTopicRepository([]);
        var clock = new TestClock(NowUtc);
        var useCase = new SetTopic(settingsRepository, topicRepository, clock);
        var command = new SetTopicCommand(1, 99);

        Assert.That(
            async () => await useCase.ExecuteAsync(command, CancellationToken.None),
            Throws.InvalidOperationException);
    }

    [Test]
    public async Task ExecuteAsync_NullTopic_ClearsSelection()
    {
        var settingsRepository = new FakeUserSettingsRepository();
        var existing = new UserSettings(1, NowUtc);
        existing.SelectTopic(5, NowUtc);
        settingsRepository.Seed(existing);
        var topicRepository = new FakeTopicRepository([]);
        var clock = new TestClock(NowUtc);
        var useCase = new SetTopic(settingsRepository, topicRepository, clock);
        var command = new SetTopicCommand(1, null);

        await useCase.ExecuteAsync(command, CancellationToken.None);

        var updated = await settingsRepository.GetOrCreateAsync(1, NowUtc, CancellationToken.None);
        Assert.That(updated.SelectedTopicId, Is.Null);
        Assert.That(settingsRepository.UpdateCallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task ExecuteAsync_ExistingTopic_SavesSelection()
    {
        var settingsRepository = new FakeUserSettingsRepository();
        var topicRepository = new FakeTopicRepository(new[] { new TopicListItem(3, "Еда", 10) });
        var clock = new TestClock(NowUtc);
        var useCase = new SetTopic(settingsRepository, topicRepository, clock);
        var command = new SetTopicCommand(1, 3);

        await useCase.ExecuteAsync(command, CancellationToken.None);

        var updated = await settingsRepository.GetOrCreateAsync(1, NowUtc, CancellationToken.None);
        Assert.That(updated.SelectedTopicId, Is.EqualTo(3));
        Assert.That(settingsRepository.UpdateCallCount, Is.EqualTo(1));
    }
}
