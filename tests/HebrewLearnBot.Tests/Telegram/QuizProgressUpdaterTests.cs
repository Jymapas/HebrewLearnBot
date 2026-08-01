using HebrewLearnBot.Application.Quiz;
using HebrewLearnBot.Worker.Telegram;

namespace HebrewLearnBot.Tests.Telegram;

[TestFixture]
public sealed class QuizProgressUpdaterTests
{
    private static QuizConversationData BuildData(
        int wordId,
        int askedCount = 0,
        int correctCount = 0,
        IEnumerable<int>? recentWordIds = null,
        IEnumerable<int>? wrongWordIds = null)
    {
        return new QuizConversationData
        {
            Mode = 1,
            TopicId = null,
            QuestionToken = "token",
            QuestionMessageId = 100,
            WordId = wordId,
            CorrectIndex = 0,
            OptionWordIds = [wordId, wordId + 1, wordId + 2, wordId + 3],
            OptionLabels = ["a", "b", "c", "d"],
            AskedCount = askedCount,
            CorrectCount = correctCount,
            RecentWordIds = recentWordIds?.ToList() ?? [],
            WrongWordIds = wrongWordIds?.ToList() ?? []
        };
    }

    [Test]
    public void Update_CorrectAnswer_IncrementsBothCounters()
    {
        var data = BuildData(wordId: 5, askedCount: 3, correctCount: 2);

        var updated = QuizProgressUpdater.Update(data, isCorrect: true);

        Assert.That(updated.AskedCount, Is.EqualTo(4));
        Assert.That(updated.CorrectCount, Is.EqualTo(3));
    }

    [Test]
    public void Update_WrongAnswer_AddsWordToWrongList()
    {
        var data = BuildData(wordId: 7, wrongWordIds: [1, 2]);

        var updated = QuizProgressUpdater.Update(data, isCorrect: false);

        Assert.That(updated.AskedCount, Is.EqualTo(1));
        Assert.That(updated.CorrectCount, Is.EqualTo(0));
        Assert.That(updated.WrongWordIds, Is.EqualTo(new List<int> { 1, 2, 7 }));
    }

    [Test]
    public void Update_RecentListExceedsWindow_KeepsOnlyLastItems()
    {
        var existingRecent = Enumerable.Range(1, QuizConstants.RecentWindowSize).ToList();
        var data = BuildData(wordId: 999, recentWordIds: existingRecent);

        var updated = QuizProgressUpdater.Update(data, isCorrect: true);

        Assert.That(updated.RecentWordIds, Has.Count.EqualTo(QuizConstants.RecentWindowSize));

        var expected = existingRecent.Skip(1).Append(999).ToList();
        Assert.That(updated.RecentWordIds, Is.EqualTo(expected));
        Assert.That(updated.RecentWordIds[^1], Is.EqualTo(999));
    }

    [Test]
    public void Update_WrongListReachesCap_StopsGrowing()
    {
        var existingWrong = Enumerable.Range(1, QuizConstants.MaxWrongWordsTracked).ToList();
        var data = BuildData(wordId: 12345, wrongWordIds: existingWrong);

        var updated = QuizProgressUpdater.Update(data, isCorrect: false);

        Assert.That(updated.WrongWordIds, Has.Count.EqualTo(QuizConstants.MaxWrongWordsTracked));
        Assert.That(updated.WrongWordIds, Does.Not.Contain(12345));
        Assert.That(updated.WrongWordIds, Is.EqualTo(existingWrong));
    }
}
