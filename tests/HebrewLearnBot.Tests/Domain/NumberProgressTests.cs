using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Domain.Exceptions;

namespace HebrewLearnBot.Tests.Domain;

public sealed class NumberProgressTests
{
    private static readonly DateTime Now = new(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc);

    [TestCase(-1)]
    [TestCase(100)]
    public void Constructor_InvalidValue_Throws(int value)
    {
        Assert.That(() => new NumberProgress(1, value, NumberQuizMode.HebrewToNumber, Now), Throws.TypeOf<DomainException>());
    }

    [Test]
    public void Constructor_InvalidModeOrRestrictedRange_Throws()
    {
        Assert.That(() => new NumberProgress(1, 0, (NumberQuizMode)99, Now), Throws.TypeOf<DomainException>());
        Assert.That(() => new NumberProgress(1, 11, NumberQuizMode.NumberToHebrewSixOptions, Now), Throws.TypeOf<DomainException>());
    }

    [Test]
    public void RegisterAnswer_LearnedThenWrong_ResetsLearnedAndRecordsTimestamp()
    {
        var progress = new NumberProgress(1, 0, NumberQuizMode.HebrewToNumber, Now);
        for (var i = 0; i < 5; i++)
        {
            progress.RegisterAnswer(true, Now);
        }

        Assert.That(progress.IsLearned, Is.True);
        progress.RegisterAnswer(false, Now.AddMinutes(1));
        Assert.That(progress.IsLearned, Is.False);
        Assert.That(progress.CorrectCount, Is.EqualTo(5));
        Assert.That(progress.WrongCount, Is.EqualTo(1));
        Assert.That(progress.Streak, Is.Zero);
        Assert.That(progress.LastShownAtUtc, Is.EqualTo(Now.AddMinutes(1)));
        Assert.That(progress.LastAnswerCorrect, Is.False);
    }

    [Test]
    public void RegisterSkip_AfterAnswer_KeepsCountersAndResetsStreak()
    {
        var progress = new NumberProgress(1, 99, NumberQuizMode.TransliterationToNumberInput, Now);
        progress.RegisterAnswer(true, Now);
        progress.RegisterSkip(Now.AddMinutes(1));
        Assert.That(progress.CorrectCount, Is.EqualTo(1));
        Assert.That(progress.WrongCount, Is.Zero);
        Assert.That(progress.Streak, Is.Zero);
        Assert.That(progress.LastAnswerCorrect, Is.Null);
        Assert.That(progress.UpdatedAtUtc, Is.EqualTo(Now.AddMinutes(1)));
    }
}
