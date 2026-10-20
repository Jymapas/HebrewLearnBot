using HebrewLearnBot.Application.UseCases.Numbers;
using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Domain.Numbers;
using HebrewLearnBot.Tests.Fakes;

namespace HebrewLearnBot.Tests.UseCases;

public sealed class NumberQuizTests
{
    private static readonly DateTime Now = new(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc);

    [TestCase(NumberQuizMode.HebrewToNumber, 4, 99)]
    [TestCase(NumberQuizMode.TransliterationToNumber, 4, 99)]
    [TestCase(NumberQuizMode.NumberToHebrewWithTransliteration, 4, 99)]
    [TestCase(NumberQuizMode.NumberToHebrew, 4, 99)]
    [TestCase(NumberQuizMode.NumberToHebrewSixOptions, 6, 10)]
    public async Task ExecuteAsync_ChoiceMode_BuildsUniqueOptionsContainingCorrectAnswer(NumberQuizMode mode, int optionCount, int maxValue)
    {
        var builder = new BuildNextNumberQuestion(new FakeNumberProgressRepository(), new SequenceRandomSource(123), new TestClock(Now));
        for (var i = 0; i < 30; i++)
        {
            var question = await builder.ExecuteAsync(1, mode, [], i, 0, CancellationToken.None);
            Assert.That(question.Number.Value, Is.InRange(0, maxValue));
            Assert.That(question.Options.Count, Is.EqualTo(optionCount));
            Assert.That(question.Options.Select(item => item.Value).Distinct().Count(), Is.EqualTo(optionCount));
            Assert.That(question.Options.Select(item => item.Label).Distinct().Count(), Is.EqualTo(optionCount));
            Assert.That(question.Options[question.CorrectIndex].Value, Is.EqualTo(question.Number.Value));
            Assert.That(question.Options.All(item => item.Value <= maxValue), Is.True);
        }
    }

    [TestCase(NumberQuizMode.HebrewToNumberInput)]
    [TestCase(NumberQuizMode.TransliterationToNumberInput)]
    public async Task ExecuteAsync_InputMode_HasNoAnswerButtons(NumberQuizMode mode)
    {
        var builder = new BuildNextNumberQuestion(new FakeNumberProgressRepository(), new SequenceRandomSource(123), new TestClock(Now));
        var question = await builder.ExecuteAsync(1, mode, [], 0, 0, CancellationToken.None);
        Assert.That(question.Options, Is.Empty);
        Assert.That(question.CorrectIndex, Is.EqualTo(-1));
        Assert.That(question.QuestionText, Does.Contain(mode == NumberQuizMode.HebrewToNumberInput ? question.Number.Hebrew : question.Number.Transliteration));
    }

    [Test]
    public async Task ExecuteAsync_RecentQuestions_ExcludesLastFiveNumbers()
    {
        var builder = new BuildNextNumberQuestion(new FakeNumberProgressRepository(), new SequenceRandomSource(123), new TestClock(Now));
        int[] recent = [0, 1, 2, 3, 4];
        for (var i = 0; i < 30; i++)
        {
            var question = await builder.ExecuteAsync(1, NumberQuizMode.NumberToHebrewSixOptions, recent, 0, 0, CancellationToken.None);
            Assert.That(recent, Does.Not.Contain(question.Number.Value));
        }
    }

    [Test]
    public async Task ExecuteAsync_ModeProgress_DoesNotChangeAnotherModeSelection()
    {
        var repository = new FakeNumberProgressRepository();
        await new SubmitNumberAnswer(repository, new TestClock(Now)).ExecuteAsync(1, 0, NumberQuizMode.HebrewToNumber, false, CancellationToken.None);
        var withOtherMode = new BuildNextNumberQuestion(repository, new SequenceRandomSource(123), new TestClock(Now));
        var empty = new BuildNextNumberQuestion(new FakeNumberProgressRepository(), new SequenceRandomSource(123), new TestClock(Now));
        var first = await withOtherMode.ExecuteAsync(1, NumberQuizMode.TransliterationToNumber, [], 0, 0, CancellationToken.None);
        var second = await empty.ExecuteAsync(1, NumberQuizMode.TransliterationToNumber, [], 0, 0, CancellationToken.None);
        Assert.That(first.Number, Is.EqualTo(second.Number));
        Assert.That(first.Options, Is.EqualTo(second.Options));
        Assert.That(first.CorrectIndex, Is.EqualTo(second.CorrectIndex));
    }

    [Test]
    public async Task ExecuteAsync_LearnedAndWrongNumbers_FavorsWrongAndNewNumbers()
    {
        var repository = new FakeNumberProgressRepository();
        var clock = new TestClock(Now);
        var submit = new SubmitNumberAnswer(repository, clock);
        for (var value = 0; value < 50; value++)
        {
            for (var answer = 0; answer < 5; answer++)
            {
                await submit.ExecuteAsync(1, value, NumberQuizMode.HebrewToNumberInput, true, CancellationToken.None);
            }
        }

        await submit.ExecuteAsync(1, 50, NumberQuizMode.HebrewToNumberInput, false, CancellationToken.None);
        var builder = new BuildNextNumberQuestion(repository, new SequenceRandomSource(123), clock);
        var learnedCount = 0;
        var wrongCount = 0;
        for (var i = 0; i < 1000; i++)
        {
            var question = await builder.ExecuteAsync(1, NumberQuizMode.HebrewToNumberInput, [], 0, 0, CancellationToken.None);
            learnedCount += question.Number.Value < 50 ? 1 : 0;
            wrongCount += question.Number.Value == 50 ? 1 : 0;
        }

        Assert.That(learnedCount, Is.LessThan(150));
        Assert.That(wrongCount, Is.GreaterThan(15));
    }

    [Test]
    public async Task ExecuteAsync_SubmitAndSkip_PersistUserModeAndClockIndependently()
    {
        var repository = new FakeNumberProgressRepository();
        var clock = new TestClock(Now);
        var submit = new SubmitNumberAnswer(repository, clock);
        await submit.ExecuteAsync(1, 0, NumberQuizMode.HebrewToNumber, true, CancellationToken.None);
        clock.UtcNow = Now.AddMinutes(1);
        await submit.ExecuteAsync(1, 0, NumberQuizMode.TransliterationToNumberInput, false, CancellationToken.None);
        await new SkipNumberQuestion(repository, clock).ExecuteAsync(2, 99, NumberQuizMode.HebrewToNumber, CancellationToken.None);
        Assert.That(repository.Progress.Count, Is.EqualTo(3));
        Assert.That(repository.Progress[0].CorrectCount, Is.EqualTo(1));
        Assert.That(repository.Progress[0].WrongCount, Is.Zero);
        Assert.That(repository.Progress[1].WrongCount, Is.EqualTo(1));
        Assert.That(repository.Progress[1].LastShownAtUtc, Is.EqualTo(clock.UtcNow));
        Assert.That(repository.Progress[2].LastAnswerCorrect, Is.Null);
    }

    [Test]
    public async Task ExecuteAsync_Statistics_CountsNumbersOnceAcrossModes()
    {
        var repository = new FakeNumberProgressRepository();
        var submit = new SubmitNumberAnswer(repository, new TestClock(Now));
        foreach (var mode in new[] { NumberQuizMode.HebrewToNumber, NumberQuizMode.NumberToHebrew })
        {
            for (var i = 0; i < 5; i++)
            {
                await submit.ExecuteAsync(1, 0, mode, true, CancellationToken.None);
            }
        }

        await submit.ExecuteAsync(1, 99, NumberQuizMode.TransliterationToNumberInput, false, CancellationToken.None);
        await submit.ExecuteAsync(2, 50, NumberQuizMode.HebrewToNumber, true, CancellationToken.None);
        var statistics = await new GetNumberStatistics(repository).ExecuteAsync(1, CancellationToken.None);
        Assert.That(statistics.TotalNumbers, Is.EqualTo(HebrewNumbers.All.Count));
        Assert.That(statistics.SeenNumbers, Is.EqualTo(2));
        Assert.That(statistics.LearnedNumbers, Is.EqualTo(1));
        Assert.That(statistics.TotalAnswers, Is.EqualTo(11));
        Assert.That(statistics.CorrectAnswers, Is.EqualTo(10));
    }
}
