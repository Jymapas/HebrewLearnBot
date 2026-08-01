using HebrewLearnBot.Application.Abstractions.Randomness;

namespace HebrewLearnBot.Tests.Fakes;

public sealed class SequenceRandomSource : IRandomSource
{
    private readonly double[]? _values;
    private readonly Random? _random;
    private int _index;

    public SequenceRandomSource(params double[] values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        _values = values;
    }

    public SequenceRandomSource(int seed)
    {
        _random = new Random(seed);
    }

    public double NextDouble()
    {
        if (_random is not null)
        {
            return _random.NextDouble();
        }

        // Значения отдаём по кругу, чтобы источник никогда не «заканчивался».
        var value = _values![_index % _values.Length];
        _index++;
        return value;
    }

    public int Next(int maxExclusive)
    {
        if (_random is not null)
        {
            return _random.Next(maxExclusive);
        }

        var scaled = (int)(NextDouble() * maxExclusive);
        return Math.Clamp(scaled, 0, maxExclusive - 1);
    }
}
