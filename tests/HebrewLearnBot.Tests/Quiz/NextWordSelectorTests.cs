using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Quiz;
using HebrewLearnBot.Tests.Fakes;

namespace HebrewLearnBot.Tests.Quiz;

public sealed class NextWordSelectorTests
{
    private static readonly DateTime NowUtc = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Select_EmptyCandidates_ReturnsNull()
    {
        var selector = new NextWordSelector(new WeightedPicker(new SequenceRandomSource(0.5)));

        var result = selector.Select(Array.Empty<QuizCandidate>(), Array.Empty<int>(), NowUtc);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Select_RecentWord_IsAlmostNeverPicked()
    {
        var candidates = new List<QuizCandidate>();
        for (var wordId = 1; wordId <= 5; wordId++)
        {
            candidates.Add(new CandidateBuilder().WithWordId(wordId).WithProgress(false).Build());
        }

        var recentWordIds = new[] { 1 };
        var selector = new NextWordSelector(new WeightedPicker(new SequenceRandomSource(seed: 99)));
        var pickCounts = new Dictionary<int, int>();

        for (var i = 0; i < 1000; i++)
        {
            var picked = selector.Select(candidates, recentWordIds, NowUtc);
            pickCounts[picked!.WordId] = pickCounts.GetValueOrDefault(picked.WordId) + 1;
        }

        Assert.That(pickCounts.GetValueOrDefault(1), Is.LessThan(50));
    }

    [Test]
    public void Select_SingleCandidate_ReturnsItEvenIfRecent()
    {
        var candidate = new CandidateBuilder().WithWordId(42).Build();
        var selector = new NextWordSelector(new WeightedPicker(new SequenceRandomSource(0.5)));

        var result = selector.Select(new[] { candidate }, new[] { 42 }, NowUtc);

        Assert.That(result, Is.EqualTo(candidate));
    }

    [Test]
    public void Select_NewWordAmongLearnedOnes_IsPickedMostOften()
    {
        var learnedCandidates = new List<QuizCandidate>();
        for (var wordId = 1; wordId <= 4; wordId++)
        {
            learnedCandidates.Add(new CandidateBuilder()
                .WithWordId(wordId)
                .WithProgress()
                .WithCorrectCount(10)
                .WithWrongCount(0)
                .WithStreak(10)
                .WithLastShownAtUtc(NowUtc)
                .WithLastAnswerCorrect(true)
                .Build());
        }

        var newWord = new CandidateBuilder().WithWordId(99).WithProgress(false).Build();
        var candidates = new List<QuizCandidate>(learnedCandidates) { newWord };
        var selector = new NextWordSelector(new WeightedPicker(new SequenceRandomSource(seed: 5)));
        var pickCounts = new Dictionary<int, int>();

        for (var i = 0; i < 500; i++)
        {
            var picked = selector.Select(candidates, Array.Empty<int>(), NowUtc);
            pickCounts[picked!.WordId] = pickCounts.GetValueOrDefault(picked.WordId) + 1;
        }

        var newWordCount = pickCounts.GetValueOrDefault(99);
        var maxLearnedCount = learnedCandidates.Max(c => pickCounts.GetValueOrDefault(c.WordId));

        Assert.That(newWordCount, Is.GreaterThan(maxLearnedCount));
    }
}
