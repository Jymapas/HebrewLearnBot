using HebrewLearnBot.Application.Alphabet;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Quiz;
using HebrewLearnBot.Tests.Fakes;

namespace HebrewLearnBot.Tests.Alphabet;

public sealed class LetterNextSelectorTests
{
    private static readonly DateTime NowUtc = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Select_EmptyCandidates_ReturnsNull()
    {
        var selector = new LetterNextSelector(new WeightedPicker(new SequenceRandomSource(0.5)));

        var result = selector.Select(Array.Empty<LetterCandidate>(), Array.Empty<int>(), NowUtc);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Select_RecentLetter_IsAlmostNeverPicked()
    {
        var candidates = new List<LetterCandidate>();
        for (var letterId = 1; letterId <= 5; letterId++)
        {
            candidates.Add(new LetterCandidateBuilder().WithLetterId(letterId).WithProgress(false).Build());
        }

        var recentLetterIds = new[] { 1 };
        var selector = new LetterNextSelector(new WeightedPicker(new SequenceRandomSource(seed: 99)));
        var pickCounts = new Dictionary<int, int>();

        for (var i = 0; i < 1000; i++)
        {
            var picked = selector.Select(candidates, recentLetterIds, NowUtc);
            pickCounts[picked!.LetterId] = pickCounts.GetValueOrDefault(picked.LetterId) + 1;
        }

        Assert.That(pickCounts.GetValueOrDefault(1), Is.LessThan(50));
    }

    [Test]
    public void Select_SingleCandidate_ReturnsItEvenIfRecent()
    {
        var candidate = new LetterCandidateBuilder().WithLetterId(42).Build();
        var selector = new LetterNextSelector(new WeightedPicker(new SequenceRandomSource(0.5)));

        var result = selector.Select(new[] { candidate }, new[] { 42 }, NowUtc);

        Assert.That(result, Is.EqualTo(candidate));
    }

    [Test]
    public void Select_NewLetterAmongLearnedOnes_IsPickedMostOften()
    {
        var learnedCandidates = new List<LetterCandidate>();
        for (var letterId = 1; letterId <= 4; letterId++)
        {
            learnedCandidates.Add(new LetterCandidateBuilder()
                .WithLetterId(letterId)
                .WithProgress()
                .WithCorrectCount(10)
                .WithWrongCount(0)
                .WithStreak(10)
                .WithLastShownAtUtc(NowUtc)
                .WithLastAnswerCorrect(true)
                .Build());
        }

        var newLetter = new LetterCandidateBuilder().WithLetterId(99).WithProgress(false).Build();
        var candidates = new List<LetterCandidate>(learnedCandidates) { newLetter };
        var selector = new LetterNextSelector(new WeightedPicker(new SequenceRandomSource(seed: 5)));
        var pickCounts = new Dictionary<int, int>();

        for (var i = 0; i < 500; i++)
        {
            var picked = selector.Select(candidates, Array.Empty<int>(), NowUtc);
            pickCounts[picked!.LetterId] = pickCounts.GetValueOrDefault(picked.LetterId) + 1;
        }

        var newLetterCount = pickCounts.GetValueOrDefault(99);
        var maxLearnedCount = learnedCandidates.Max(c => pickCounts.GetValueOrDefault(c.LetterId));

        Assert.That(newLetterCount, Is.GreaterThan(maxLearnedCount));
    }
}
