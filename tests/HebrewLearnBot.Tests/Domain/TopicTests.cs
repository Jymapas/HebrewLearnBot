using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Domain.Exceptions;

namespace HebrewLearnBot.Tests.Domain;

public sealed class TopicTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Topic_NameLongerThanLimit_ThrowsDomainException()
    {
        var longName = new string('a', 101);

        Assert.That(() => new Topic(longName, CreatedAtUtc), Throws.TypeOf<DomainException>());
    }
}
