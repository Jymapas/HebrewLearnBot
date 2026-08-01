using HebrewLearnBot.Domain.Entities;

namespace HebrewLearnBot.Tests.Domain;

public sealed class WordProgressTests
{
    private static readonly DateTime CreatedAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void WordProgress_RegisterAnswerCorrect_IncrementsCorrectCountAndStreak()
    {
        var progress = new WordProgress(1, 1, CreatedAtUtc);
        var answeredAtUtc = CreatedAtUtc.AddMinutes(1);

        progress.RegisterAnswer(true, answeredAtUtc);

        Assert.That(progress.CorrectCount, Is.EqualTo(1));
        Assert.That(progress.Streak, Is.EqualTo(1));
        Assert.That(progress.LastAnswerCorrect, Is.True);
        Assert.That(progress.LastShownAtUtc, Is.EqualTo(answeredAtUtc));
    }

    [Test]
    public void WordProgress_RegisterAnswerWrong_ResetsStreakAndIncrementsWrongCount()
    {
        var progress = new WordProgress(1, 1, CreatedAtUtc);
        progress.RegisterAnswer(true, CreatedAtUtc.AddMinutes(1));
        progress.RegisterAnswer(true, CreatedAtUtc.AddMinutes(2));

        progress.RegisterAnswer(false, CreatedAtUtc.AddMinutes(3));

        Assert.That(progress.WrongCount, Is.EqualTo(1));
        Assert.That(progress.Streak, Is.EqualTo(0));
        Assert.That(progress.LastAnswerCorrect, Is.False);
    }

    [Test]
    public void WordProgress_RegisterSkip_UpdatesLastShownWithoutTouchingCounters()
    {
        var progress = new WordProgress(1, 1, CreatedAtUtc);
        progress.RegisterAnswer(true, CreatedAtUtc.AddMinutes(1));
        var skippedAtUtc = CreatedAtUtc.AddMinutes(2);

        progress.RegisterSkip(skippedAtUtc);

        Assert.That(progress.Streak, Is.EqualTo(0));
        Assert.That(progress.LastAnswerCorrect, Is.Null);
        Assert.That(progress.LastShownAtUtc, Is.EqualTo(skippedAtUtc));
        Assert.That(progress.CorrectCount, Is.EqualTo(1));
        Assert.That(progress.WrongCount, Is.EqualTo(0));
    }

    [Test]
    public void WordProgress_IsLearned_TrueOnlyWhenFiveCorrectAndStreakThree()
    {
        // Негативный случай: пять правильных ответов есть, но серия прервана неправильным ответом.
        var brokenStreakProgress = new WordProgress(1, 1, CreatedAtUtc);
        brokenStreakProgress.RegisterAnswer(true, CreatedAtUtc.AddMinutes(1));
        brokenStreakProgress.RegisterAnswer(true, CreatedAtUtc.AddMinutes(2));
        brokenStreakProgress.RegisterAnswer(true, CreatedAtUtc.AddMinutes(3));
        brokenStreakProgress.RegisterAnswer(false, CreatedAtUtc.AddMinutes(4));
        brokenStreakProgress.RegisterAnswer(true, CreatedAtUtc.AddMinutes(5));
        brokenStreakProgress.RegisterAnswer(true, CreatedAtUtc.AddMinutes(6));

        Assert.That(brokenStreakProgress.CorrectCount, Is.EqualTo(5));
        Assert.That(brokenStreakProgress.Streak, Is.EqualTo(2));
        Assert.That(brokenStreakProgress.IsLearned, Is.False);

        // Позитивный случай: пять правильных ответов подряд без сбоев.
        var learnedProgress = new WordProgress(2, 2, CreatedAtUtc);
        for (var i = 1; i <= 5; i++)
        {
            learnedProgress.RegisterAnswer(true, CreatedAtUtc.AddMinutes(i));
        }

        Assert.That(learnedProgress.IsLearned, Is.True);
    }
}
