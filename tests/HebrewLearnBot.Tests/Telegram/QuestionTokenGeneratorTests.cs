using System.Text.RegularExpressions;
using HebrewLearnBot.Worker.Telegram;

namespace HebrewLearnBot.Tests.Telegram;

[TestFixture]
public sealed class QuestionTokenGeneratorTests
{
    [Test]
    public void Generate_ReturnsEightUrlSafeCharacters()
    {
        var token = QuestionTokenGenerator.Generate();

        Assert.That(token, Has.Length.EqualTo(8));
        Assert.That(Regex.IsMatch(token, "^[A-Za-z0-9_-]+$"), Is.True);
    }

    [Test]
    public void Generate_CalledTwice_ReturnsDifferentTokens()
    {
        var first = QuestionTokenGenerator.Generate();
        var second = QuestionTokenGenerator.Generate();

        Assert.That(first, Is.Not.EqualTo(second));
    }
}
