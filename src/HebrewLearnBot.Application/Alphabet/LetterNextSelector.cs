using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Quiz;

namespace HebrewLearnBot.Application.Alphabet;

public sealed class LetterNextSelector
{
    private readonly WeightedPicker _picker;

    public LetterNextSelector(WeightedPicker picker)
    {
        _picker = picker;
    }

    public LetterCandidate? Select(
        IReadOnlyList<LetterCandidate> candidates,
        IReadOnlyCollection<int> recentLetterIds,
        DateTime nowUtc)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        var effectiveWindow = Math.Min(QuizConstants.RecentWindowSize, candidates.Count - 1);
        var recentTail = BuildRecentTail(recentLetterIds, effectiveWindow);

        var weights = new double[candidates.Count];
        for (var i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            var weight = LetterWeightCalculator.Calculate(candidate, nowUtc);

            if (effectiveWindow > 0 && recentTail.Contains(candidate.LetterId))
            {
                weight *= QuizConstants.RecentPenalty;
            }

            weights[i] = weight;
        }

        var index = _picker.PickIndex(weights);
        return candidates[index];
    }

    private static HashSet<int> BuildRecentTail(IReadOnlyCollection<int> recentLetterIds, int effectiveWindow)
    {
        var tail = new HashSet<int>();
        if (effectiveWindow <= 0)
        {
            return tail;
        }

        // recentLetterIds приходит «самые свежие в конце» — берём хвост нужной длины.
        var recentList = recentLetterIds as IReadOnlyList<int> ?? recentLetterIds.ToList();
        var start = Math.Max(0, recentList.Count - effectiveWindow);
        for (var i = start; i < recentList.Count; i++)
        {
            tail.Add(recentList[i]);
        }

        return tail;
    }
}
