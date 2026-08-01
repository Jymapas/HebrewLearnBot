using HebrewLearnBot.Application.UseCases.Quiz;
using HebrewLearnBot.Tests.Fakes;

namespace HebrewLearnBot.Tests.UseCases;

public sealed class SubmitQuizAnswerTests
{
    private static readonly DateTime NowUtc = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public async Task ExecuteAsync_CorrectAnswer_RegistersCorrectAnswer()
    {
        var repository = new FakeWordProgressRepository();
        var clock = new TestClock(NowUtc);
        var useCase = new SubmitQuizAnswer(repository, clock);
        var command = new SubmitQuizAnswerCommand(123, 45, true);

        await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(repository.RegisteredAnswers, Has.Count.EqualTo(1));
        var (userId, wordId, isCorrect, atUtc) = repository.RegisteredAnswers[0];
        Assert.That(userId, Is.EqualTo(123));
        Assert.That(wordId, Is.EqualTo(45));
        Assert.That(isCorrect, Is.True);
        Assert.That(atUtc, Is.EqualTo(NowUtc));
    }

    [Test]
    public async Task ExecuteAsync_UsesClockNotSystemTime()
    {
        var fixedTime = new DateTime(2020, 5, 1, 8, 30, 0, DateTimeKind.Utc);
        var repository = new FakeWordProgressRepository();
        var clock = new TestClock(fixedTime);
        var useCase = new SubmitQuizAnswer(repository, clock);
        var command = new SubmitQuizAnswerCommand(1, 2, false);

        await useCase.ExecuteAsync(command, CancellationToken.None);

        Assert.That(repository.RegisteredAnswers[0].AtUtc, Is.EqualTo(fixedTime));
    }
}
