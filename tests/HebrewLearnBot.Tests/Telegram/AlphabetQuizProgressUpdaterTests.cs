using HebrewLearnBot.Application.Quiz;
using HebrewLearnBot.Worker.Telegram;

namespace HebrewLearnBot.Tests.Telegram;

[TestFixture]
public sealed class AlphabetQuizProgressUpdaterTests
{
    private static AlphabetQuizConversationData BuildData(
        int letterId,
        int askedCount = 0,
        int correctCount = 0,
        IEnumerable<int>? recentLetterIds = null,
        IEnumerable<int>? wrongLetterIds = null)
    {
        return new AlphabetQuizConversationData
        {
            QuestionToken = "token",
            QuestionMessageId = 100,
            LetterId = letterId,
            CorrectIndex = 0,
            OptionLetterIds = [letterId, letterId + 1, letterId + 2, letterId + 3],
            OptionLabels = ["a", "b", "c", "d"],
            AskedCount = askedCount,
            CorrectCount = correctCount,
            RecentLetterIds = recentLetterIds?.ToList() ?? [],
            WrongLetterIds = wrongLetterIds?.ToList() ?? []
        };
    }

    [Test]
    public void Update_CorrectAnswer_IncrementsBothCounters()
    {
        var data = BuildData(letterId: 5, askedCount: 3, correctCount: 2);

        var updated = AlphabetQuizProgressUpdater.Update(data, isCorrect: true);

        Assert.That(updated.AskedCount, Is.EqualTo(4));
        Assert.That(updated.CorrectCount, Is.EqualTo(3));
    }

    [Test]
    public void Update_WrongAnswer_AddsLetterToWrongList()
    {
        var data = BuildData(letterId: 7, wrongLetterIds: [1, 2]);

        var updated = AlphabetQuizProgressUpdater.Update(data, isCorrect: false);

        Assert.That(updated.AskedCount, Is.EqualTo(1));
        Assert.That(updated.CorrectCount, Is.EqualTo(0));
        Assert.That(updated.WrongLetterIds, Is.EqualTo(new List<int> { 1, 2, 7 }));
    }

    [Test]
    public void Update_RecentListExceedsWindow_KeepsOnlyLastItems()
    {
        var existingRecent = Enumerable.Range(1, QuizConstants.RecentWindowSize).ToList();
        var data = BuildData(letterId: 999, recentLetterIds: existingRecent);

        var updated = AlphabetQuizProgressUpdater.Update(data, isCorrect: true);

        Assert.That(updated.RecentLetterIds, Has.Count.EqualTo(QuizConstants.RecentWindowSize));

        var expected = existingRecent.Skip(1).Append(999).ToList();
        Assert.That(updated.RecentLetterIds, Is.EqualTo(expected));
        Assert.That(updated.RecentLetterIds[^1], Is.EqualTo(999));
    }

    [Test]
    public void Update_WrongListReachesCap_StopsGrowing()
    {
        var existingWrong = Enumerable.Range(1, QuizConstants.MaxWrongWordsTracked).ToList();
        var data = BuildData(letterId: 12345, wrongLetterIds: existingWrong);

        var updated = AlphabetQuizProgressUpdater.Update(data, isCorrect: false);

        Assert.That(updated.WrongLetterIds, Has.Count.EqualTo(QuizConstants.MaxWrongWordsTracked));
        Assert.That(updated.WrongLetterIds, Does.Not.Contain(12345));
        Assert.That(updated.WrongLetterIds, Is.EqualTo(existingWrong));
    }
}
