using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Worker.Telegram;
using Telegram.Bot.Types.ReplyMarkups;

namespace HebrewLearnBot.Tests.Telegram;

[TestFixture]
public sealed class InlineKeyboardsTests
{
    private static List<List<InlineKeyboardButton>> Flatten(InlineKeyboardMarkup markup)
    {
        return markup.InlineKeyboard.Select(row => row.ToList()).ToList();
    }

    [Test]
    public void AnsweredQuizOptions_CorrectButton_GetsSuccessStyle()
    {
        var labels = new List<string> { "Один", "Два", "Три" };

        var markup = InlineKeyboards.AnsweredQuizOptions(labels, correctIndex: 1, chosenIndex: 2);
        var buttons = Flatten(markup).SelectMany(row => row).ToList();

        Assert.That(buttons[1].Style, Is.EqualTo(KeyboardButtonStyle.Success));
    }

    [Test]
    public void AnsweredQuizOptions_ChosenWrongButton_GetsDangerStyle()
    {
        var labels = new List<string> { "Один", "Два", "Три" };

        var markup = InlineKeyboards.AnsweredQuizOptions(labels, correctIndex: 1, chosenIndex: 2);
        var buttons = Flatten(markup).SelectMany(row => row).ToList();

        Assert.That(buttons[2].Style, Is.EqualTo(KeyboardButtonStyle.Danger));
    }

    [Test]
    public void AnsweredQuizOptions_UntouchedButtons_HaveNoStyle()
    {
        var labels = new List<string> { "Один", "Два", "Три" };

        var markup = InlineKeyboards.AnsweredQuizOptions(labels, correctIndex: 1, chosenIndex: 2);
        var buttons = Flatten(markup).SelectMany(row => row).ToList();

        Assert.That(buttons[0].Style, Is.Null);
    }

    [Test]
    public void AnsweredQuizOptions_AllButtons_CarryNoopCallbackData()
    {
        var labels = new List<string> { "Один", "Два", "Три" };

        var markup = InlineKeyboards.AnsweredQuizOptions(labels, correctIndex: 1, chosenIndex: 2);
        var buttons = Flatten(markup).SelectMany(row => row).ToList();

        Assert.That(buttons, Has.All.Property("CallbackData").EqualTo(CallbackData.Noop));
    }

    [Test]
    public void AnsweredQuizOptions_CorrectAnswer_HasNoDangerButton()
    {
        var labels = new List<string> { "Один", "Два", "Три" };

        var markup = InlineKeyboards.AnsweredQuizOptions(labels, correctIndex: 1, chosenIndex: 1);
        var buttons = Flatten(markup).SelectMany(row => row).ToList();

        Assert.That(buttons.Any(b => b.Style == KeyboardButtonStyle.Danger), Is.False);
        Assert.That(buttons.Count(b => b.Style == KeyboardButtonStyle.Success), Is.EqualTo(1));
    }

    [Test]
    public void AnsweredQuizOptions_SkippedQuestion_HasOnlySuccessStyle()
    {
        var labels = new List<string> { "Один", "Два", "Три" };

        var markup = InlineKeyboards.AnsweredQuizOptions(labels, correctIndex: 1, chosenIndex: -1);
        var buttons = Flatten(markup).SelectMany(row => row).ToList();

        Assert.That(buttons.Count(b => b.Style == KeyboardButtonStyle.Success), Is.EqualTo(1));
        Assert.That(buttons.Any(b => b.Style == KeyboardButtonStyle.Danger), Is.False);
        Assert.That(buttons[1].Style, Is.EqualTo(KeyboardButtonStyle.Success));
    }

    [Test]
    public void QuizOptions_BuildsOneButtonPerRowWithSequentialIndices()
    {
        var labels = new List<string> { "Один", "Два", "Три", "Четыре" };

        var markup = InlineKeyboards.QuizOptions("Xk3pQ9zA", labels);
        var rows = Flatten(markup);

        Assert.That(rows, Has.Count.EqualTo(4));
        Assert.That(rows, Has.All.Matches<List<InlineKeyboardButton>>(row => row.Count == 1));

        for (var i = 0; i < labels.Count; i++)
        {
            var expected = CallbackData.BuildQuizAnswer("Xk3pQ9zA", i);
            Assert.That(rows[i][0].CallbackData, Is.EqualTo(expected));
        }
    }

    [Test]
    public void ModePicker_CurrentMode_IsMarkedWithCheck()
    {
        var markup = InlineKeyboards.ModePicker(QuizMode.RuToHe);
        var buttons = Flatten(markup).SelectMany(row => row).ToList();

        var currentButton = buttons.Single(b => b.CallbackData == CallbackData.BuildMode(QuizMode.RuToHe));
        Assert.That(currentButton.Text, Does.StartWith("✅"));

        var otherButtons = buttons.Where(b => b.CallbackData != CallbackData.BuildMode(QuizMode.RuToHe));
        Assert.That(otherButtons, Has.All.Matches<InlineKeyboardButton>(b => !b.Text.StartsWith("✅")));
    }

    [Test]
    public void TopicPicker_MoreThanOnePage_RendersNavigationRow()
    {
        var topics = Enumerable.Range(1, 9)
            .Select(i => new TopicListItem(i, $"Тема {i}", i * 2))
            .ToList();

        var markup = InlineKeyboards.TopicPicker(topics, selectedTopicId: null, page: 0, totalWordCount: 100);
        var rows = Flatten(markup);

        var navigationRow = rows.SingleOrDefault(row =>
            row.Any(b => b.CallbackData != null && b.CallbackData.StartsWith(CallbackData.TopicPagePrefix)));

        Assert.That(navigationRow, Is.Not.Null);
    }
}
