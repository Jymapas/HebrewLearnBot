namespace HebrewLearnBot.Application.Abstractions.Randomness;

public interface IRandomSource
{
    double NextDouble();

    int Next(int maxExclusive);
}
