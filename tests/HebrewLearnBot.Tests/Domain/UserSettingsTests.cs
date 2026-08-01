using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Domain.Exceptions;

namespace HebrewLearnBot.Tests.Domain;

public sealed class UserSettingsTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime UpdatedAtUtc = new(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void UserSettings_SelectTopicWithZero_ThrowsDomainException()
    {
        var settings = new UserSettings(1, CreatedAtUtc);

        Assert.That(() => settings.SelectTopic(0, UpdatedAtUtc), Throws.TypeOf<DomainException>());
    }

    [Test]
    public void UserSettings_NewInstance_DefaultsToHeToRu()
    {
        var settings = new UserSettings(1, CreatedAtUtc);

        Assert.That(settings.SelectedMode, Is.EqualTo(QuizMode.HeToRu));
        Assert.That(settings.SelectedTopicId, Is.Null);
    }
}
