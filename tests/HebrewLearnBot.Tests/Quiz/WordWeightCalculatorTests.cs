using HebrewLearnBot.Application.Quiz;

namespace HebrewLearnBot.Tests.Quiz;

public sealed class WordWeightCalculatorTests
{
    private static readonly DateTime NowUtc = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Calculate_NewWord_ReturnsNewWordWeight()
    {
        var candidate = new CandidateBuilder().WithProgress(false).Build();

        var weight = WordWeightCalculator.Calculate(candidate, NowUtc);

        Assert.That(weight, Is.EqualTo(QuizConstants.NewWordWeight).Within(1e-9));
    }

    [Test]
    public void Calculate_WordAnsweredWrongJustNow_ReturnsExpectedWeight()
    {
        var candidate = new CandidateBuilder()
            .WithProgress()
            .WithCorrectCount(0)
            .WithWrongCount(1)
            .WithStreak(0)
            .WithLastShownAtUtc(NowUtc)
            .WithLastAnswerCorrect(false)
            .Build();

        var weight = WordWeightCalculator.Calculate(candidate, NowUtc);

        Assert.That(weight, Is.EqualTo(4.5).Within(1e-6));
    }

    [Test]
    public void Calculate_MixedCountersThreeDaysAgo_ReturnsExpectedWeight()
    {
        var candidate = new CandidateBuilder()
            .WithProgress()
            .WithCorrectCount(2)
            .WithWrongCount(2)
            .WithStreak(0)
            .WithLastShownAtUtc(NowUtc.AddDays(-3))
            .WithLastAnswerCorrect(true)
            .Build();

        var weight = WordWeightCalculator.Calculate(candidate, NowUtc);

        var expected = 1.25 * (1.0 + 3.0 / 7.0);
        Assert.That(weight, Is.EqualTo(expected).Within(1e-6));
        Assert.That(weight, Is.EqualTo(1.7857).Within(1e-3));
    }

    [Test]
    public void Calculate_CleanWordSeenYesterday_WeighsLessThanOne()
    {
        var candidate = new CandidateBuilder()
            .WithProgress()
            .WithCorrectCount(3)
            .WithWrongCount(0)
            .WithStreak(3)
            .WithLastShownAtUtc(NowUtc.AddDays(-1))
            .WithLastAnswerCorrect(true)
            .Build();

        var weight = WordWeightCalculator.Calculate(candidate, NowUtc);

        Assert.That(weight, Is.LessThan(1.0));
        Assert.That(weight, Is.EqualTo(0.2078).Within(1e-3));
    }

    [Test]
    public void Calculate_LearnedWord_IsDampened()
    {
        var candidate = new CandidateBuilder()
            .WithProgress()
            .WithCorrectCount(7)
            .WithWrongCount(0)
            .WithStreak(4)
            .WithLastShownAtUtc(NowUtc.AddDays(-10))
            .WithLastAnswerCorrect(true)
            .Build();

        Assert.That(candidate.IsLearned, Is.True);

        var weight = WordWeightCalculator.Calculate(candidate, NowUtc);

        Assert.That(weight, Is.EqualTo(0.0739).Within(1e-3));
    }

    [Test]
    public void Calculate_WordWithErrors_WeighsMoreThanCleanWord()
    {
        var lastShownAtUtc = NowUtc.AddDays(-2);
        var cleanWord = new CandidateBuilder()
            .WithProgress()
            .WithCorrectCount(3)
            .WithWrongCount(0)
            .WithStreak(3)
            .WithLastShownAtUtc(lastShownAtUtc)
            .WithLastAnswerCorrect(true)
            .Build();
        var wordWithErrors = new CandidateBuilder()
            .WithProgress()
            .WithCorrectCount(3)
            .WithWrongCount(2)
            .WithStreak(0)
            .WithLastShownAtUtc(lastShownAtUtc)
            .WithLastAnswerCorrect(true)
            .Build();

        var cleanWeight = WordWeightCalculator.Calculate(cleanWord, NowUtc);
        var errorsWeight = WordWeightCalculator.Calculate(wordWithErrors, NowUtc);

        Assert.That(errorsWeight, Is.GreaterThan(cleanWeight));
    }

    [Test]
    public void Calculate_SpacingCapsAtFourteenDays()
    {
        var baseCandidate = new CandidateBuilder()
            .WithProgress()
            .WithCorrectCount(2)
            .WithWrongCount(1)
            .WithStreak(1)
            .WithLastAnswerCorrect(true)
            .Build();
        var candidateAtCap = baseCandidate with { LastShownAtUtc = NowUtc.AddDays(-14) };
        var candidateBeyondCap = baseCandidate with { LastShownAtUtc = NowUtc.AddDays(-100) };

        var weightAtCap = WordWeightCalculator.Calculate(candidateAtCap, NowUtc);
        var weightBeyondCap = WordWeightCalculator.Calculate(candidateBeyondCap, NowUtc);

        Assert.That(weightBeyondCap, Is.EqualTo(weightAtCap).Within(1e-9));
    }

    [Test]
    public void Calculate_ExtremeCounters_ClampsToConfiguredRange()
    {
        var highWeightCandidate = new CandidateBuilder()
            .WithProgress()
            .WithCorrectCount(0)
            .WithWrongCount(1000)
            .WithStreak(0)
            .WithLastShownAtUtc(NowUtc.AddDays(-100))
            .WithLastAnswerCorrect(false)
            .Build();
        var lowWeightCandidate = new CandidateBuilder()
            .WithProgress()
            .WithCorrectCount(1000)
            .WithWrongCount(0)
            .WithStreak(1000)
            .WithLastShownAtUtc(NowUtc)
            .WithLastAnswerCorrect(true)
            .Build();

        var highWeight = WordWeightCalculator.Calculate(highWeightCandidate, NowUtc);
        var lowWeight = WordWeightCalculator.Calculate(lowWeightCandidate, NowUtc);

        Assert.That(highWeight, Is.EqualTo(QuizConstants.MaxWeight).Within(1e-9));
        Assert.That(lowWeight, Is.EqualTo(QuizConstants.MinWeight).Within(1e-9));
    }
}
