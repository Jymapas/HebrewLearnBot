using HebrewLearnBot.Application.Abstractions.Time;

namespace HebrewLearnBot.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
