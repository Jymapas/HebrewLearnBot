using System.Net;
using System.Text;
using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.UseCases.Numbers;
using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Tests.Fakes;
using HebrewLearnBot.Worker.Config;
using HebrewLearnBot.Worker.Telegram;
using HebrewLearnBot.Worker.Telegram.Numbers;
using Microsoft.Extensions.Logging.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HebrewLearnBot.Tests.Telegram;

[TestFixture]
public sealed class NumberQuizHandlerTests
{
    private StateRepository _states = null!;
    private FakeNumberProgressRepository _progress = null!;
    private BotTransport _transport = null!;
    private NumberQuizHandler _handler = null!;
    private TestClock _clock = null!;
    private NumberQuizMenuStore _menus = null!;
    private HttpClient _httpClient = null!;

    [SetUp]
    public void SetUp()
    {
        _states = new StateRepository();
        _progress = new FakeNumberProgressRepository();
        _transport = new BotTransport();
        _httpClient = new HttpClient(_transport);
        _clock = new TestClock(new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc));
        _menus = new NumberQuizMenuStore(_clock);
        _handler = new NumberQuizHandler(
            new TelegramBotClient("123456:TEST_TOKEN", _httpClient), _states, _clock, _menus,
            new BuildNextNumberQuestion(_progress, new SequenceRandomSource(42), _clock),
            new SubmitNumberAnswer(_progress, _clock), new SkipNumberQuestion(_progress, _clock),
            new GetNumberStatistics(_progress), new AdminGuard(new AppOptions { MainAdminId = 999 }),
            NullLogger<NumberQuizHandler>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
        _transport.Dispose();
    }

    [Test]
    public async Task TryHandleMessageAsync_InvalidNumber_LeavesQuestionActive()
    {
        var data = await SeedAsync(NumberQuizMode.HebrewToNumberInput);
        var result = await _handler.TryHandleMessageAsync(Message("сто"), CancellationToken.None);
        Assert.That(result, Is.True);
        Assert.That(_states.State!.StateJson, Is.EqualTo(NumberQuizConversationJson.Serialize(data)));
        Assert.That(_progress.Progress, Is.Empty);
    }

    [TestCase(10, false)]
    [TestCase(30, true)]
    public async Task TryHandleMessageAsync_StaleText_DoesNotCountAnswer(int messageId, bool oldReply)
    {
        await SeedAsync(NumberQuizMode.HebrewToNumberInput);
        var message = Message("3");
        message.Id = messageId;
        if (oldReply)
        {
            message.ReplyToMessage = new Message { Id = 9 };
        }
        await _handler.TryHandleMessageAsync(message, CancellationToken.None);
        Assert.That(_progress.Progress, Is.Empty);
        Assert.That(_states.State!.Step, Is.EqualTo("answer"));
    }

    [Test]
    public async Task TryHandleMessageAsync_Stop_DeletesStateAndShowsSummary()
    {
        await SeedAsync(NumberQuizMode.HebrewToNumberInput);
        await _handler.TryHandleMessageAsync(Message(BotCommands.Stop), CancellationToken.None);
        Assert.That(_states.State, Is.Null);
        Assert.That(_progress.Progress, Is.Empty);
        using var json = System.Text.Json.JsonDocument.Parse(_transport.Bodies.Last());
        Assert.That(json.RootElement.GetProperty("text").GetString(), Does.Contain("Тренировка завершена"));
    }

    [Test]
    public async Task TryHandleMessageAsync_Skip_RecordsSkipAndMovesToNextQuestion()
    {
        await SeedAsync(NumberQuizMode.NumberToHebrewSixOptions);
        await _handler.TryHandleMessageAsync(Message(MenuButtons.QuizSkip), CancellationToken.None);
        var data = NumberQuizConversationJson.Deserialize(_states.State!.StateJson)!;
        Assert.That(data.AskedCount, Is.EqualTo(1));
        Assert.That(data.CorrectCount, Is.Zero);
        Assert.That(data.Value, Is.Not.EqualTo(3));
        Assert.That(data.Labels, Has.Count.EqualTo(6));
        Assert.That(_progress.Progress.Single().LastShownAtUtc, Is.EqualTo(_clock.UtcNow));
        Assert.That(_progress.Progress.Single().CorrectCount, Is.Zero);
    }

    [TestCase("old", 10, 1)]
    [TestCase("token", 9, 1)]
    [TestCase("token", 10, 2)]
    public async Task TryHandleCallbackAsync_StaleTokenOrMessageOrChat_AcknowledgesWithoutCounting(string token, int messageId, long chatId)
    {
        await SeedAsync(NumberQuizMode.NumberToHebrewSixOptions);
        await _handler.TryHandleCallbackAsync(Callback(token, messageId, chatId), CancellationToken.None);
        Assert.That(_progress.Progress, Is.Empty);
        Assert.That(_transport.Paths, Has.Some.EndsWith("answerCallbackQuery"));
    }

    [Test]
    public async Task TryHandleCallbackAsync_RepeatedAnswer_CountsOnlyOnce()
    {
        await SeedAsync(NumberQuizMode.NumberToHebrewSixOptions);
        await _handler.TryHandleCallbackAsync(Callback("token", 10, 1), CancellationToken.None);
        await _handler.TryHandleCallbackAsync(Callback("token", 10, 1), CancellationToken.None);
        Assert.That(_progress.Progress.Sum(item => item.CorrectCount + item.WrongCount), Is.EqualTo(1));
        Assert.That(_transport.Paths.Count(path => path.EndsWith("answerCallbackQuery", StringComparison.Ordinal)), Is.EqualTo(2));
    }

    [Test]
    public async Task TryHandleMessageAsync_OpenMenu_PreservesExistingQuiz()
    {
        await _states.UpsertAsync(1, ConversationNames.Quiz, "answer", "existing", _clock.UtcNow, CancellationToken.None);
        await _handler.TryHandleMessageAsync(Message(MenuButtons.Numbers), CancellationToken.None);
        Assert.That(_states.State!.ConversationName, Is.EqualTo(ConversationNames.Quiz));
        Assert.That(_states.State.StateJson, Is.EqualTo("existing"));
        Assert.That(_menus.Get(1), Is.Not.Null);
    }

    [Test]
    public async Task TryHandleMessageAsync_LongSessionSummary_FitsTelegramLimit()
    {
        var data = await SeedAsync(NumberQuizMode.HebrewToNumberInput);
        data.WrongNumbers = Enumerable.Range(0, 100).ToList();
        data.AskedCount = 100;
        await _states.UpsertAsync(1, NumberQuizHandler.ConversationName, "answer", NumberQuizConversationJson.Serialize(data), _clock.UtcNow, CancellationToken.None);
        await _handler.TryHandleMessageAsync(Message(BotCommands.Stop), CancellationToken.None);
        using var json = System.Text.Json.JsonDocument.Parse(_transport.Bodies.Last());
        var text = json.RootElement.GetProperty("text").GetString()!;
        Assert.That(text.Length, Is.LessThan(4096));
        Assert.That(text, Does.Contain("И ещё 80 чисел"));
    }

    [Test]
    public async Task TryHandleMessageAsync_RepeatedSkip_DoesNotSkipNextUnseenQuestion()
    {
        await SeedAsync(NumberQuizMode.NumberToHebrewSixOptions);
        await _handler.TryHandleMessageAsync(Message(MenuButtons.QuizSkip), CancellationToken.None);
        await _handler.TryHandleMessageAsync(Message(MenuButtons.QuizSkip), CancellationToken.None);
        var data = NumberQuizConversationJson.Deserialize(_states.State!.StateJson)!;
        Assert.That(data.AskedCount, Is.EqualTo(1));
        Assert.That(_progress.Progress, Has.Count.EqualTo(1));
    }

    [TestCase("3", 1)]
    [TestCase("4", 0)]
    public async Task TryHandleMessageAsync_ValidNumber_RecordsAnswerAndSendsNext(string answer, int correctCount)
    {
        await SeedAsync(NumberQuizMode.HebrewToNumberInput);
        await _handler.TryHandleMessageAsync(Message(answer), CancellationToken.None);
        Assert.That(_progress.Progress.Single().CorrectCount, Is.EqualTo(correctCount));
        Assert.That(_progress.Progress.Single().WrongCount, Is.EqualTo(1 - correctCount));
        var data = NumberQuizConversationJson.Deserialize(_states.State!.StateJson)!;
        Assert.That(data.AskedCount, Is.EqualTo(1));
        Assert.That(data.CorrectCount, Is.EqualTo(correctCount));
        Assert.That(data.Value, Is.Not.EqualTo(3));
    }

    [TestCase(NumberQuizMode.NumberToHebrewSixOptions, 6)]
    [TestCase(NumberQuizMode.HebrewToNumberInput, 0)]
    public async Task TryHandleCallbackAsync_MenuMode_StartsExpectedTraining(NumberQuizMode mode, int optionCount)
    {
        await _handler.TryHandleMessageAsync(Message(MenuButtons.Numbers), CancellationToken.None);
        var menu = _menus.Get(1)!;
        var callback = Callback(menu.Token, menu.MessageId, 1);
        callback.Data = $"num:{menu.Token}:{(int)mode}";
        await _handler.TryHandleCallbackAsync(callback, CancellationToken.None);
        var data = NumberQuizConversationJson.Deserialize(_states.State!.StateJson)!;
        Assert.That(data.Mode, Is.EqualTo(mode));
        Assert.That(data.Labels, Has.Count.EqualTo(optionCount));
        Assert.That(data.Value, Is.InRange(0, mode == NumberQuizMode.NumberToHebrewSixOptions ? 10 : 99));
        Assert.That(_menus.Get(1), Is.Null);
    }

    private async Task<NumberQuizConversationData> SeedAsync(NumberQuizMode mode)
    {
        var data = new NumberQuizConversationData
        {
            ChatId = 1, MessageId = 10, Token = "token", Mode = mode, Value = 3,
            CorrectIndex = 0, Labels = ["שלוש", "אחת", "שתיים", "ארבע", "חמש", "שש"]
        };
        await _states.UpsertAsync(1, NumberQuizHandler.ConversationName, "answer", NumberQuizConversationJson.Serialize(data), _clock.UtcNow, CancellationToken.None);
        return data;
    }

    private static Message Message(string text)
    {
        return new Message { Id = 20, From = new User { Id = 1, FirstName = "Test" }, Chat = new Chat { Id = 1 }, Text = text };
    }

    private static CallbackQuery Callback(string token, int messageId, long chatId)
    {
        return new CallbackQuery
        {
            Id = "callback", From = new User { Id = 1, FirstName = "Test" }, Data = $"num:{token}:a0",
            Message = new Message { Id = messageId, Chat = new Chat { Id = chatId } }
        };
    }

    private sealed class StateRepository : IConversationStateRepository
    {
        public ConversationState? State { get; private set; }

        public Task<ConversationState?> GetByUserIdAsync(long userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(State?.UserId == userId ? State : null);
        }

        public Task UpsertAsync(long userId, string conversationName, string step, string? stateJson, DateTime nowUtc, CancellationToken cancellationToken)
        {
            State = new ConversationState(userId, conversationName, step, stateJson, nowUtc);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(long userId, CancellationToken cancellationToken)
        {
            State = null;
            return Task.CompletedTask;
        }
    }

    private sealed class BotTransport : HttpMessageHandler
    {
        public List<string> Paths { get; } = [];
        public List<string> Bodies { get; } = [];
        private int _messageId = 100;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;
            Paths.Add(path);
            Bodies.Add(request.Content is null ? "" : await request.Content.ReadAsStringAsync(cancellationToken));
            var result = path.EndsWith("answerCallbackQuery", StringComparison.Ordinal)
                ? "true"
                : $$"""{"message_id":{{++_messageId}},"date":1792454400,"chat":{"id":1,"type":"private"},"text":"test"}""";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true,\"result\":" + result + "}", Encoding.UTF8, "application/json")
            };
        }
    }
}
