using HebrewLearnBot.Application.Numbers;

namespace HebrewLearnBot.Tests.Numbers;

public sealed class NumberQuizRulesTests
{
    [TestCase("0", 0)]
    [TestCase("99", 99)]
    [TestCase(" 12 ", 12)]
    [TestCase("08", 8)]
    public void TryParseAnswer_DecimalInRange_Parses(string text, int expected)
    {
        Assert.That(NumberQuizRules.TryParseAnswer(text, out var value), Is.True);
        Assert.That(value, Is.EqualTo(expected));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("100")]
    [TestCase("999999999999999999999")]
    [TestCase("-1")]
    [TestCase("+1")]
    [TestCase("1.0")]
    [TestCase("1 2")]
    [TestCase("אחת")]
    [TestCase("١")]
    public void TryParseAnswer_InvalidText_Rejects(string? text)
    {
        Assert.That(NumberQuizRules.TryParseAnswer(text, out _), Is.False);
    }
}
