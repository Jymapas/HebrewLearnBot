using System.Text;
using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Worker.Telegram;

namespace HebrewLearnBot.Tests.Telegram;

[TestFixture]
public sealed class CallbackDataTests
{
    [Test]
    public void BuildQuizAnswer_TypicalValues_StaysUnder64Bytes()
    {
        var token = QuestionTokenGenerator.Generate();
        var data = CallbackData.BuildQuizAnswer(token, 3);

        Assert.That(Encoding.UTF8.GetByteCount(data), Is.LessThanOrEqualTo(64));
    }

    [Test]
    public void TryParseQuizAnswer_ValidPayload_ReturnsTokenAndIndex()
    {
        var data = CallbackData.BuildQuizAnswer("Xk3pQ9zA", 2);

        var result = CallbackData.TryParseQuizAnswer(data, out var token, out var index);

        Assert.That(result, Is.True);
        Assert.That(token, Is.EqualTo("Xk3pQ9zA"));
        Assert.That(index, Is.EqualTo(2));
    }

    [Test]
    public void TryParseQuizAnswer_Garbage_ReturnsFalse()
    {
        var result = CallbackData.TryParseQuizAnswer("not-a-callback", out _, out _);

        Assert.That(result, Is.False);
    }

    [Test]
    public void TryParseQuizAnswer_Null_ReturnsFalse()
    {
        var result = CallbackData.TryParseQuizAnswer(null, out _, out _);

        Assert.That(result, Is.False);
    }

    [Test]
    public void BuildAlphabetAnswer_TypicalValues_StaysUnder64Bytes()
    {
        var token = QuestionTokenGenerator.Generate();
        var data = CallbackData.BuildAlphabetAnswer(token, 3);

        Assert.That(Encoding.UTF8.GetByteCount(data), Is.LessThanOrEqualTo(64));
    }

    [Test]
    public void TryParseAlphabetAnswer_ValidPayload_ReturnsTokenAndIndex()
    {
        var data = CallbackData.BuildAlphabetAnswer("Xk3pQ9zA", 2);

        var result = CallbackData.TryParseAlphabetAnswer(data, out var token, out var index);

        Assert.That(result, Is.True);
        Assert.That(token, Is.EqualTo("Xk3pQ9zA"));
        Assert.That(index, Is.EqualTo(2));
    }

    [Test]
    public void TryParseAlphabetAnswer_QuizAnswerPayload_ReturnsFalse()
    {
        var data = CallbackData.BuildQuizAnswer("Xk3pQ9zA", 2);

        var result = CallbackData.TryParseAlphabetAnswer(data, out _, out _);

        Assert.That(result, Is.False);
    }

    [Test]
    public void TryParseAlphabetAnswer_Null_ReturnsFalse()
    {
        var result = CallbackData.TryParseAlphabetAnswer(null, out _, out _);

        Assert.That(result, Is.False);
    }

    [Test]
    public void TryParseMode_UnknownNumber_ReturnsFalse()
    {
        var result = CallbackData.TryParseMode("qm:99", out _);

        Assert.That(result, Is.False);
    }

    [Test]
    public void TryParseMode_ValidMode_ReturnsMode()
    {
        var data = CallbackData.BuildMode(QuizMode.RuToHe);

        var result = CallbackData.TryParseMode(data, out var mode);

        Assert.That(result, Is.True);
        Assert.That(mode, Is.EqualTo(QuizMode.RuToHe));
    }

    [Test]
    public void TryParseTopic_ZeroMeansAllTopics_ReturnsZero()
    {
        var data = CallbackData.BuildTopic(0);

        var result = CallbackData.TryParseTopic(data, out var topicId);

        Assert.That(result, Is.True);
        Assert.That(topicId, Is.EqualTo(0));
    }

    [Test]
    public void BuildAndParseTopicPage_RoundTrips()
    {
        var data = CallbackData.BuildTopicPage(3);

        var result = CallbackData.TryParseTopicPage(data, out var page);

        Assert.That(result, Is.True);
        Assert.That(page, Is.EqualTo(3));
    }
}
