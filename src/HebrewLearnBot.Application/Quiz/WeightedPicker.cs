using HebrewLearnBot.Application.Abstractions.Randomness;

namespace HebrewLearnBot.Application.Quiz;

public sealed class WeightedPicker
{
    private readonly IRandomSource _randomSource;

    public WeightedPicker(IRandomSource randomSource)
    {
        _randomSource = randomSource;
    }

    public int PickIndex(IReadOnlyList<double> weights)
    {
        if (weights.Count == 0)
        {
            throw new ArgumentException("Weights must not be empty.", nameof(weights));
        }

        var hasPositiveWeight = false;
        var bestIndex = 0;
        var bestKey = double.PositiveInfinity;

        for (var i = 0; i < weights.Count; i++)
        {
            var weight = weights[i];
            if (weight <= 0.0)
            {
                continue;
            }

            hasPositiveWeight = true;

            var key = NextKey(weight);
            if (key < bestKey)
            {
                bestKey = key;
                bestIndex = i;
            }
        }

        // Вырожденный случай: все веса нулевые/отрицательные — берём равновероятный индекс.
        if (!hasPositiveWeight)
        {
            return _randomSource.Next(weights.Count);
        }

        return bestIndex;
    }

    public IReadOnlyList<T> PickDistinct<T>(IReadOnlyList<T> items, IReadOnlyList<double> weights, int count)
    {
        if (items.Count == 0 || count <= 0)
        {
            return Array.Empty<T>();
        }

        var keyedIndexes = new (double Key, int Index)[items.Count];
        for (var i = 0; i < items.Count; i++)
        {
            var weight = weights[i];
            keyedIndexes[i] = (weight <= 0.0 ? double.PositiveInfinity : NextKey(weight), i);
        }

        Array.Sort(keyedIndexes, static (a, b) => a.Key.CompareTo(b.Key));

        var takeCount = Math.Min(count, items.Count);
        var result = new List<T>(takeCount);
        for (var i = 0; i < takeCount; i++)
        {
            result.Add(items[keyedIndexes[i].Index]);
        }

        return result;
    }

    public void Shuffle<T>(IList<T> items)
    {
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = _randomSource.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }

    private double NextKey(double weight)
    {
        // Алгоритм Efraimidis–Spirakis: ключ -ln(U)/weight, минимальный ключ побеждает.
        var u = _randomSource.NextDouble();
        if (u <= 0.0)
        {
            u = double.Epsilon;
        }

        return -Math.Log(u) / weight;
    }
}
