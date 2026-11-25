using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Application.Formatting;
using HebrewLearnBot.Application.Numbers;
using HebrewLearnBot.Application.UseCases.Numbers;
using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Domain.Numbers;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace HebrewLearnBot.Worker.Telegram.Numbers;

public sealed class NumberQuizHandler
{
    public const string ConversationName = ConversationNames.NumberQuiz;
    private const string AnswerStep = "answer";
    private const string ConsumedStep = "consumed";
    private readonly ITelegramBotClient _botClient;
    private readonly IConversationStateRepository _states;
    private readonly IClock _clock;
    private readonly NumberQuizMenuStore _menus;
    private readonly BuildNextNumberQuestion _buildQuestion;
    private readonly SubmitNumberAnswer _submitAnswer;
    private readonly SkipNumberQuestion _skipQuestion;
    private readonly GetNumberStatistics _statistics;
    private readonly AdminGuard _adminGuard;
    private readonly ILogger<NumberQuizHandler> _logger;

    public NumberQuizHandler(
        ITelegramBotClient botClient,
        IConversationStateRepository states,
        IClock clock,
        NumberQuizMenuStore menus,
        BuildNextNumberQuestion buildQuestion,
        SubmitNumberAnswer submitAnswer,
        SkipNumberQuestion skipQuestion,
        GetNumberStatistics statistics,
        AdminGuard adminGuard,
        ILogger<NumberQuizHandler> logger)
    {
        _botClient = botClient;
        _states = states;
        _clock = clock;
        _menus = menus;
        _buildQuestion = buildQuestion;
        _submitAnswer = submitAnswer;
        _skipQuestion = skipQuestion;
        _statistics = statistics;
        _adminGuard = adminGuard;
        _logger = logger;
    }

    public async Task<bool> TryHandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        if (message.From is null)
        {
            return false;
        }

        var userId = message.From.Id;
        var chatId = message.Chat.Id;
        var text = message.Text?.Trim();
        var command = text?.Split(' ', '\n')[0].Split('@')[0];
        if (text == MenuButtons.Numbers || command == BotCommands.Numbers)
        {
            await ShowMenuAsync(chatId, userId, "menu", cancellationToken);
            return true;
        }

        var state = await _states.GetByUserIdAsync(userId, cancellationToken);
        if (state?.ConversationName != ConversationName
            || NumberQuizConversationJson.Deserialize(state.StateJson) is not { } data
            || data.ChatId != chatId)
        {
            return false;
        }

        if (text == MenuButtons.QuizStop || command == BotCommands.Stop)
        {
            await FinishAsync(userId, data, cancellationToken);
            return true;
        }

        if (text == MenuButtons.QuizSkip)
        {
            if (state.Step == AnswerStep && !string.IsNullOrEmpty(data.Token) && message.MessageId > data.MessageId)
            {
                await CompleteAnswerAsync(userId, data, -1, false, true, cancellationToken);
            }
            return true;
        }

        if (text is null || text.StartsWith('/') || IsNavigation(text))
        {
            return false;
        }

        if (state.Step == AnswerStep && NumberQuizRules.IsTextInput(data.Mode) && !string.IsNullOrEmpty(data.Token))
        {
            if (message.MessageId <= data.MessageId
                || (message.ReplyToMessage is not null && message.ReplyToMessage.MessageId != data.MessageId))
            {
                await _botClient.SendNoPreview(chatId, "Этот ответ относится к предыдущему вопросу. Ответьте на текущий.", cancellationToken: cancellationToken);
                return true;
            }

            if (!NumberQuizRules.TryParseAnswer(text, out var value))
            {
                await _botClient.SendNoPreview(chatId, "Введите целое число от 0 до 99. Вопрос остаётся активным.", cancellationToken: cancellationToken);
                return true;
            }

            await CompleteAnswerAsync(userId, data, -1, value == data.Value, false, cancellationToken);
            return true;
        }

        await _botClient.SendNoPreview(chatId, "Выберите ответ на инлайн-клавиатуре.", cancellationToken: cancellationToken);
        return true;
    }

    public async Task<bool> TryHandleCallbackAsync(CallbackQuery callback, CancellationToken cancellationToken)
    {
        if (callback.Data?.StartsWith(NumberQuizKeyboards.Prefix, StringComparison.Ordinal) != true)
        {
            return false;
        }

        // Подтверждаем до обработки: исключение при чтении FSM не оставит спиннер.
        await _botClient.AnswerCallbackQuery(callback.Id, cancellationToken: cancellationToken);
        var parts = callback.Data.Split(':');
        var menu = _menus.Get(callback.From.Id);
        if (parts.Length != 3 || callback.Message is null)
        {
            return true;
        }
        var action = parts[2];
        if (menu is not null && menu.Token == parts[1] && menu.ChatId == callback.Message.Chat.Id
            && menu.MessageId == callback.Message.MessageId)
        {
            if (action is "cards" or "input" or "menu")
            {
                _menus.Remove(callback.From.Id);
                await ShowMenuAsync(menu.ChatId, callback.From.Id, action, cancellationToken);
            }
            else if (int.TryParse(action, out var modeValue) && Enum.IsDefined(typeof(NumberQuizMode), modeValue))
            {
                _menus.Remove(callback.From.Id);
                menu.Mode = (NumberQuizMode)modeValue;
                await SendNextAsync(callback.From.Id, menu, cancellationToken);
                await _botClient.SendNoPreview(menu.ChatId,
                    NumberQuizRules.IsTextInput(menu.Mode) ? "Введите ответ числом. Для завершения нажмите «⏹ Стоп»." : "Отвечайте кнопками. Для завершения нажмите «⏹ Стоп».",
                    ReplyKeyboards.QuizMenu(), cancellationToken);
            }
            return true;
        }
        var state = await _states.GetByUserIdAsync(callback.From.Id, cancellationToken);
        if (state?.ConversationName != ConversationName
            || NumberQuizConversationJson.Deserialize(state.StateJson) is not { } data
            || string.IsNullOrEmpty(data.Token) || parts[1] != data.Token
            || callback.Message.Chat.Id != data.ChatId || callback.Message.MessageId != data.MessageId)
        {
            return true;
        }

        if (state.Step == AnswerStep && !NumberQuizRules.IsTextInput(data.Mode)
            && action.StartsWith('a') && int.TryParse(action.AsSpan(1), out var index)
            && index >= 0 && index < data.Labels.Count)
        {
            await CompleteAnswerAsync(callback.From.Id, data, index, index == data.CorrectIndex, false, cancellationToken);
        }
        return true;
    }

    public async Task AppendStatisticsAsync(long chatId, long userId, CancellationToken cancellationToken)
    {
        var statistics = await _statistics.ExecuteAsync(userId, cancellationToken);
        await _botClient.SendNoPreview(chatId, NumberStatisticsFormatter.Build(statistics), cancellationToken: cancellationToken);
    }

    private async Task ShowMenuAsync(long chatId, long userId, string page, CancellationToken cancellationToken)
    {
        var data = new NumberQuizConversationData { ChatId = chatId, Token = QuestionTokenGenerator.Generate() };
        var state = await _states.GetByUserIdAsync(userId, cancellationToken);
        var activeQuiz = state?.ConversationName is ConversationNames.Quiz or ConversationNames.AlphabetQuiz or ConversationName;
        await _botClient.SendNoPreview(chatId, "🔢 Числа в женском роде",
            activeQuiz ? ReplyKeyboards.QuizMenu() : ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)), cancellationToken);
        var sent = await _botClient.SendNoPreview(chatId,
            page == "cards" ? "Карточки 0–99: выберите направление." : page == "input" ? "Ввести число 0–99: выберите вид вопроса." : "Выберите тренировку чисел:",
            NumberQuizKeyboards.Menu(data.Token, page, isPrivateChat: chatId == userId), cancellationToken);
        data.MessageId = sent.MessageId;
        _menus.Set(userId, data);
    }

    private async Task SendNextAsync(long userId, NumberQuizConversationData data, CancellationToken cancellationToken)
    {
        var question = await _buildQuestion.ExecuteAsync(userId, data.Mode, data.RecentNumbers, data.AskedCount, data.CorrectCount, cancellationToken);
        data.Value = question.Number.Value;
        data.CorrectIndex = question.CorrectIndex;
        data.Labels = question.Options.Select(option => option.Label).ToList();
        data.Token = QuestionTokenGenerator.Generate();
        var sent = await _botClient.SendNoPreview(data.ChatId, question.QuestionText,
            NumberQuizRules.IsTextInput(data.Mode) ? null : NumberQuizKeyboards.Options(data, null), cancellationToken);
        data.MessageId = sent.MessageId;
        await SaveAsync(userId, AnswerStep, data, cancellationToken);
    }

    private async Task CompleteAnswerAsync(long userId, NumberQuizConversationData data, int chosenIndex, bool isCorrect, bool skipped, CancellationToken cancellationToken)
    {
        await ConsumeAsync(userId, data, cancellationToken);
        if (skipped)
        {
            await _skipQuestion.ExecuteAsync(userId, data.Value, data.Mode, cancellationToken);
        }
        else
        {
            await _submitAnswer.ExecuteAsync(userId, data.Value, data.Mode, isCorrect, cancellationToken);
        }

        data.AskedCount++;
        if (isCorrect)
        {
            data.CorrectCount++;
        }
        else if (!data.WrongNumbers.Contains(data.Value))
        {
            data.WrongNumbers.Add(data.Value);
        }
        data.RecentNumbers.Add(data.Value);
        data.RecentNumbers = data.RecentNumbers.TakeLast(5).ToList();
        await ShowAnswerAsync(data, chosenIndex, isCorrect, skipped, cancellationToken);
        await SendNextAsync(userId, data, cancellationToken);
    }

    private async Task ShowAnswerAsync(NumberQuizConversationData data, int chosenIndex, bool isCorrect, bool skipped, CancellationToken cancellationToken)
    {
        var number = HebrewNumbers.Get(data.Value);
        var text = (skipped ? "⏭ Пропущено" : isCorrect ? "✅ Верно!" : "❌ Неверно")
            + "\n" + BidiText.Line($"{number.Value} — {BidiText.Isolate(number.Hebrew)} ({number.Transliteration})")
            + $"\nОтветов: {data.AskedCount} · Верно: {data.CorrectCount}";
        try
        {
            await _botClient.EditNoPreview(data.ChatId, data.MessageId, text,
                NumberQuizRules.IsTextInput(data.Mode) ? null : NumberQuizKeyboards.Options(data, chosenIndex), cancellationToken);
        }
        catch (ApiRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to edit number quiz message {MessageId}", data.MessageId);
            await _botClient.SendNoPreview(data.ChatId, text, cancellationToken: cancellationToken);
        }
    }

    private async Task FinishAsync(long userId, NumberQuizConversationData data, CancellationToken cancellationToken)
    {
        await _states.DeleteAsync(userId, cancellationToken);
        var text = $"🔢 Тренировка завершена\nОтветов: {data.AskedCount}\nВерно: {data.CorrectCount}\nОшибок и пропусков: {data.AskedCount - data.CorrectCount}";
        if (data.WrongNumbers.Count > 0)
        {
            text += "\n\nПовторите:\n" + string.Join("\n", data.WrongNumbers.Take(20).Select(value =>
            {
                var number = HebrewNumbers.Get(value);
                return BidiText.Line($"{value} — {BidiText.Isolate(number.Hebrew)} ({number.Transliteration})");
            }));
            if (data.WrongNumbers.Count > 20)
            {
                text += $"\nИ ещё {data.WrongNumbers.Count - 20} чисел.";
            }
        }
        await _botClient.SendNoPreview(data.ChatId, text, ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)), cancellationToken);
    }

    private Task ConsumeAsync(long userId, NumberQuizConversationData data, CancellationToken cancellationToken)
    {
        data.Token = string.Empty;
        return SaveAsync(userId, ConsumedStep, data, cancellationToken);
    }

    private Task SaveAsync(long userId, string step, NumberQuizConversationData data, CancellationToken cancellationToken)
    {
        return _states.UpsertAsync(userId, ConversationName, step, NumberQuizConversationJson.Serialize(data), _clock.UtcNow, cancellationToken);
    }

    private static bool IsNavigation(string text)
    {
        return text is MenuButtons.Quiz or MenuButtons.Alphabet or MenuButtons.Mode or MenuButtons.Topic
            or MenuButtons.Statistics or MenuButtons.Help or MenuButtons.ImportWords or MenuButtons.Cancel;
    }
}
