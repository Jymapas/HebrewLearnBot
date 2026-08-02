using HebrewLearnBot.Application.Alphabet;
using HebrewLearnBot.Application.Quiz;

namespace HebrewLearnBot.Tests.Alphabet;

public sealed class LetterWeightCalculatorTests
{
    private static readonly DateTime NowUtc = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Calculate_NewLetter_ReturnsNewWordWeight()
    {
        var candidate = new LetterCandidateBuilder().WithProgress(false).Build();

        var weight = LetterWeightCalculator.Calculate(candidate, NowUtc);

        Assert.That(weight, Is.EqualTo(QuizConstants.NewWordWeight).Within(1e-9));
    }

    [Test]
    public void Calculate_LetterAnsweredWrongJustNow_ReturnsExpectedWeight()
    {
        var candidate = new LetterCandidateBuilder()
            .WithProgress()
            .WithCorrectCount(0)
            .WithWrongCount(1)
            .WithStreak(0)
            .WithLastShownAtUtc(NowUtc)
            .WithLastAnswerCorrect(false)
            .Build();

        var weight = LetterWeightCalculator.Calculate(candidate, NowUtc);

        Assert.That(weight, Is.EqualTo(4.5).Within(1e-6));
    }

    [Test]
    public void Calculate_LearnedLetter_IsDampened()
    {
        var candidate = new LetterCandidateBuilder()
            .WithProgress()
            .WithCorrectCount(7)
            .WithWrongCount(0)
            .WithStreak(4)
            .WithLastShownAtUtc(NowUtc.AddDays(-10))
            .WithLastAnswerCorrect(true)
            .Build();

        Assert.That(candidate.IsLearned, Is.True);

        var weight = LetterWeightCalculator.Calculate(candidate, NowUtc);

        Assert.That(weight, Is.EqualTo(0.0739).Within(1e-3));
    }

    [Test]
    public void Calculate_SpacingCapsAtFourteenDays()
    {
        var baseCandidate = new LetterCandidateBuilder()
            .WithProgress()
            .WithCorrectCount(2)
            .WithWrongCount(1)
            .WithStreak(1)
            .WithLastAnswerCorrect(true)
            .Build();
        var candidateAtCap = baseCandidate with { LastShownAtUtc = NowUtc.AddDays(-14) };
        var candidateBeyondCap = baseCandidate with { LastShownAtUtc = NowUtc.AddDays(-100) };

        var weightAtCap = LetterWeightCalculator.Calculate(candidateAtCap, NowUtc);
        var weightBeyondCap = LetterWeightCalculator.Calculate(candidateBeyondCap, NowUtc);

        Assert.That(weightBeyondCap, Is.EqualTo(weightAtCap).Within(1e-9));
    }

    [Test]
    public void Calculate_ExtremeCounters_ClampsToConfiguredRange()
    {
        var highWeightCandidate = new LetterCandidateBuilder()
            .WithProgress()
            .WithCorrectCount(0)
            .WithWrongCount(1000)
            .WithStreak(0)
            .WithLastShownAtUtc(NowUtc.AddDays(-100))
            .WithLastAnswerCorrect(false)
            .Build();
        var lowWeightCandidate = new LetterCandidateBuilder()
            .WithProgress()
            .WithCorrectCount(1000)
            .WithWrongCount(0)
            .WithStreak(1000)
            .WithLastShownAtUtc(NowUtc)
            .WithLastAnswerCorrect(true)
            .Build();

        var highWeight = LetterWeightCalculator.Calculate(highWeightCandidate, NowUtc);
        var lowWeight = LetterWeightCalculator.Calculate(lowWeightCandidate, NowUtc);

        Assert.That(highWeight, Is.EqualTo(QuizConstants.MaxWeight).Within(1e-9));
        Assert.That(lowWeight, Is.EqualTo(QuizConstants.MinWeight).Within(1e-9));
    }
}
