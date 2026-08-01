using HebrewLearnBot.Application.Abstractions.Time;

namespace HebrewLearnBot.Tests.Fakes;

public sealed class TestClock : IClock
{
    public TestClock(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; set; }
}
