using HebrewLearnBot.Application.Abstractions.Randomness;
using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Alphabet;

public sealed class LetterDistractorPicker
{
    private readonly IRandomSource _randomSource;

    public LetterDistractorPicker(IRandomSource randomSource)
    {
        _randomSource = randomSource;
    }

    // В отличие от словарного DistractorPicker здесь нет тем/частей речи для тиров
    // и нет повторяющихся названий в датасете — достаточно перемешать остальные буквы.
    public IReadOnlyList<LetterCandidate> Pick(LetterCandidate correct, IReadOnlyList<LetterCandidate> pool, int desiredCount)
    {
        if (desiredCount <= 0)
        {
            return Array.Empty<LetterCandidate>();
        }

        var eligible = new List<LetterCandidate>(pool.Count);
        foreach (var candidate in pool)
        {
            if (candidate.LetterId != correct.LetterId)
            {
                eligible.Add(candidate);
            }
        }

        Shuffle(eligible);

        return eligible.Count <= desiredCount ? eligible : eligible.GetRange(0, desiredCount);
    }

    private void Shuffle(IList<LetterCandidate> items)
    {
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = _randomSource.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }
}
