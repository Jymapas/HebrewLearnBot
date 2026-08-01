using HebrewLearnBot.Application.Abstractions.Randomness;

namespace HebrewLearnBot.Infrastructure.Randomness;

public sealed class SystemRandomSource : IRandomSource
{
    public double NextDouble()
    {
        return Random.Shared.NextDouble();
    }

    public int Next(int maxExclusive)
    {
        return Random.Shared.Next(maxExclusive);
    }
}
