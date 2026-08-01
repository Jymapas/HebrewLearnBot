using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Quiz;

public sealed class NextWordSelector
{
    private readonly WeightedPicker _picker;

    public NextWordSelector(WeightedPicker picker)
    {
        _picker = picker;
    }

    public QuizCandidate? Select(
        IReadOnlyList<QuizCandidate> candidates,
        IReadOnlyCollection<int> recentWordIds,
        DateTime nowUtc)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        var effectiveWindow = Math.Min(QuizConstants.RecentWindowSize, candidates.Count - 1);
        var recentTail = BuildRecentTail(recentWordIds, effectiveWindow);

        var weights = new double[candidates.Count];
        for (var i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            var weight = WordWeightCalculator.Calculate(candidate, nowUtc);

            if (effectiveWindow > 0 && recentTail.Contains(candidate.WordId))
            {
                weight *= QuizConstants.RecentPenalty;
            }

            weights[i] = weight;
        }

        var index = _picker.PickIndex(weights);
        return candidates[index];
    }

    private static HashSet<int> BuildRecentTail(IReadOnlyCollection<int> recentWordIds, int effectiveWindow)
    {
        var tail = new HashSet<int>();
        if (effectiveWindow <= 0)
        {
            return tail;
        }

        // recentWordIds приходит «самые свежие в конце» — берём хвост нужной длины.
        var recentList = recentWordIds as IReadOnlyList<int> ?? recentWordIds.ToList();
        var start = Math.Max(0, recentList.Count - effectiveWindow);
        for (var i = start; i < recentList.Count; i++)
        {
            tail.Add(recentList[i]);
        }

        return tail;
    }
}
