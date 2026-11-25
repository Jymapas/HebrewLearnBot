using HebrewLearnBot.Worker.Telegram.Numbers;

namespace HebrewLearnBot.Tests.Telegram;

[TestFixture]
public sealed class NumberQuizKeyboardsTests
{
    [TestCase("menu", true, 1)]
    [TestCase("menu", false, 0)]
    [TestCase("cards", true, 0)]
    [TestCase("input", true, 0)]
    public void Menu_ChatAndPage_OffersMatchingGameOnlyOnPrivateMainMenu(
        string page, bool isPrivateChat, int expectedCount)
    {
        var markup = NumberQuizKeyboards.Menu("token", page, isPrivateChat);
        var launches = markup.InlineKeyboard.SelectMany(row => row)
            .Where(button => button.WebApp is not null).ToList();

        Assert.That(launches, Has.Count.EqualTo(expectedCount));
        foreach (var launch in launches)
        {
            Assert.That(launch.WebApp!.Url, Is.EqualTo("https://jymapas.github.io/HebrewLearnBot/"));
            Assert.That(launch.CallbackData, Is.Null);
        }
    }
}
