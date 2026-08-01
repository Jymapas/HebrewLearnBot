using HebrewLearnBot.Application.UseCases.Settings;
using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Tests.Fakes;

namespace HebrewLearnBot.Tests.UseCases;

public sealed class SetQuizModeTests
{
    private static readonly DateTime NowUtc = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public async Task ExecuteAsync_ValidMode_PersistsSelection()
    {
        var settingsRepository = new FakeUserSettingsRepository();
        var clock = new TestClock(NowUtc);
        var useCase = new SetQuizMode(settingsRepository, clock);
        var command = new SetQuizModeCommand(1, QuizMode.RuToHe);

        await useCase.ExecuteAsync(command, CancellationToken.None);

        var updated = await settingsRepository.GetOrCreateAsync(1, NowUtc, CancellationToken.None);
        Assert.That(updated.SelectedMode, Is.EqualTo(QuizMode.RuToHe));
        Assert.That(settingsRepository.UpdateCallCount, Is.EqualTo(1));
    }
}
