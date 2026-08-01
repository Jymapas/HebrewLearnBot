using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Application.UseCases.Quiz;
using HebrewLearnBot.Application.UseCases.Settings;
using HebrewLearnBot.Application.UseCases.Topics;
using HebrewLearnBot.Application.UseCases.Words;
using HebrewLearnBot.Domain.Enums;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace HebrewLearnBot.Worker.Telegram.Handlers;

public sealed class CallbackHandlers
{
    private const string StaleAnswerText = "Этот вопрос уже неактуален";

    private readonly ITelegramBotClient _botClient;
    private readonly QuizPresenter _quizPresenter;
    private readonly SubmitQuizAnswer _submitQuizAnswer;
    private readonly SetQuizMode _setQuizMode;
    private readonly SetTopic _setTopic;
    private readonly GetUserSettings _getUserSettings;
    private readonly ListTopics _listTopics;
    private readonly CountWords _countWords;
    private readonly IConversationStateRepository _conversationRepository;
    private readonly AdminGuard _adminGuard;
    private readonly IClock _clock;
    private readonly ILogger<CallbackHandlers> _logger;

    public CallbackHandlers(
        ITelegramBotClient botClient,
        QuizPresenter quizPresenter,
        SubmitQuizAnswer submitQuizAnswer,
        SetQuizMode setQuizMode,
        SetTopic setTopic,
        GetUserSettings getUserSettings,
        ListTopics listTopics,
        CountWords countWords,
        IConversationStateRepository conversationRepository,
        AdminGuard adminGuard,
        IClock clock,
        ILogger<CallbackHandlers> logger)
    {
        _botClient = botClient;
        _quizPresenter = quizPresenter;
        _submitQuizAnswer = submitQuizAnswer;
        _setQuizMode = setQuizMode;
        _setTopic = setTopic;
        _getUserSettings = getUserSettings;
        _listTopics = listTopics;
        _countWords = countWords;
        _conversationRepository = conversationRepository;
        _adminGuard = adminGuard;
        _clock = clock;
        _logger = logger;
    }

    public async Task HandleAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Handling callback {CallbackData} from user {UserId}",
                callbackQuery.Data,
                callbackQuery.From.Id);

            var data = callbackQuery.Data;

            if (data == CallbackData.Noop)
            {
                await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            if (CallbackData.TryParseQuizAnswer(data, out var questionToken, out var optionIndex))
            {
                await HandleQuizAnswerAsync(callbackQuery, questionToken, optionIndex, cancellationToken);
                return;
            }

            if (CallbackData.TryParseMode(data, out var mode))
            {
                await HandleModeAsync(callbackQuery, mode, cancellationToken);
                return;
            }

            if (CallbackData.TryParseTopic(data, out var topicId))
            {
                await HandleTopicAsync(callbackQuery, topicId, cancellationToken);
                return;
            }

            if (CallbackData.TryParseTopicPage(data, out var page))
            {
                await HandleTopicPageAsync(callbackQuery, page, cancellationToken);
                return;
            }

            if (data == CallbackData.MainMenu)
            {
                await HandleMainMenuAsync(callbackQuery, cancellationToken);
                return;
            }

            await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
            _logger.LogWarning("Unknown callback data {Data}", data);
        }
        catch (Exception ex)
        {
            // Упавший обработчик не должен оставлять спиннер пользователя висеть до таймаута.
            _logger.LogError(ex, "Unhandled exception while processing callback {CallbackId}", callbackQuery.Id);
        }
    }

    private async Task HandleQuizAnswerAsync(
        CallbackQuery callbackQuery,
        string questionToken,
        int optionIndex,
        CancellationToken cancellationToken)
    {
        var userId = callbackQuery.From.Id;
        var chatId = callbackQuery.Message?.Chat.Id;

        if (chatId is null)
        {
            await _botClient.AnswerCallbackQuery(callbackQuery.Id, StaleAnswerText, cancellationToken: cancellationToken);
            return;
        }

        var state = await _conversationRepository.GetByUserIdAsync(userId, cancellationToken);

        if (state is not { ConversationName: ConversationNames.Quiz, Step: QuizSteps.AwaitingAnswer })
        {
            await _botClient.AnswerCallbackQuery(callbackQuery.Id, StaleAnswerText, cancellationToken: cancellationToken);
            return;
        }

        var data = QuizConversationJson.Deserialize(state.StateJson);

        // Основная защита от нажатия на устаревший вопрос — совпадение QuestionToken.
        if (data is null
            || data.QuestionToken != questionToken
            || callbackQuery.Message?.MessageId != data.QuestionMessageId
            || optionIndex < 0
            || optionIndex >= data.OptionWordIds.Count)
        {
            await _botClient.AnswerCallbackQuery(callbackQuery.Id, StaleAnswerText, cancellationToken: cancellationToken);
            return;
        }

        var isCorrect = optionIndex == data.CorrectIndex;

        // Гасим токен ДО долгих операций. Запись ответа и две правки сообщения занимают
        // сотни миллисекунд, и без этого второй тап по той же кнопке успевает пройти
        // проверку выше ещё раз: засчитался бы второй ответ и пришёл бы лишний вопрос.
        data.QuestionToken = string.Empty;
        await _conversationRepository.UpsertAsync(
            userId,
            ConversationNames.Quiz,
            QuizSteps.AwaitingAnswer,
            QuizConversationJson.Serialize(data),
            _clock.UtcNow,
            cancellationToken);

        // Гасим спиннер сразу, не дожидаясь записи ответа и перерисовки сообщения.
        await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

        await _submitQuizAnswer.ExecuteAsync(new SubmitQuizAnswerCommand(userId, data.WordId, isCorrect), cancellationToken);
        await _quizPresenter.ShowAnswerAsync(chatId.Value, data, optionIndex, isCorrect, cancellationToken);

        var updated = QuizProgressUpdater.Update(data, isCorrect);
        await _quizPresenter.SendNextQuestionAsync(
            chatId.Value, userId, (QuizMode)data.Mode, data.TopicId, updated, cancellationToken);
    }

    private async Task HandleModeAsync(CallbackQuery callbackQuery, QuizMode mode, CancellationToken cancellationToken)
    {
        var userId = callbackQuery.From.Id;

        await _setQuizMode.ExecuteAsync(new SetQuizModeCommand(userId, mode), cancellationToken);
        await _botClient.AnswerCallbackQuery(callbackQuery.Id, "Режим сохранён", cancellationToken: cancellationToken);

        var chatId = callbackQuery.Message?.Chat.Id;
        var messageId = callbackQuery.Message?.MessageId;

        if (chatId is null || messageId is null)
        {
            return;
        }

        try
        {
            await _botClient.EditMessageReplyMarkup(
                chatId, messageId.Value, InlineKeyboards.ModePicker(mode), cancellationToken: cancellationToken);
        }
        catch (ApiRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to update mode picker message {MessageId}", messageId);
        }
    }

    private async Task HandleTopicAsync(CallbackQuery callbackQuery, int topicId, CancellationToken cancellationToken)
    {
        var userId = callbackQuery.From.Id;
        var selectedTopicId = topicId == 0 ? (int?)null : topicId;

        await _setTopic.ExecuteAsync(new SetTopicCommand(userId, selectedTopicId), cancellationToken);
        await _botClient.AnswerCallbackQuery(callbackQuery.Id, "Тема сохранена", cancellationToken: cancellationToken);

        var chatId = callbackQuery.Message?.Chat.Id;
        var messageId = callbackQuery.Message?.MessageId;

        if (chatId is null || messageId is null)
        {
            return;
        }

        var topics = await _listTopics.ExecuteAsync(cancellationToken);
        var totalWordCount = await _countWords.ExecuteAsync(null, cancellationToken);

        try
        {
            await _botClient.EditMessageReplyMarkup(
                chatId,
                messageId.Value,
                InlineKeyboards.TopicPicker(topics, selectedTopicId, 0, totalWordCount),
                cancellationToken: cancellationToken);
        }
        catch (ApiRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to update topic picker message {MessageId}", messageId);
        }
    }

    private async Task HandleTopicPageAsync(CallbackQuery callbackQuery, int page, CancellationToken cancellationToken)
    {
        var userId = callbackQuery.From.Id;
        var chatId = callbackQuery.Message?.Chat.Id;
        var messageId = callbackQuery.Message?.MessageId;

        await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

        if (chatId is null || messageId is null)
        {
            return;
        }

        var settings = await _getUserSettings.ExecuteAsync(userId, cancellationToken);
        var topics = await _listTopics.ExecuteAsync(cancellationToken);
        var totalWordCount = await _countWords.ExecuteAsync(null, cancellationToken);

        try
        {
            await _botClient.EditMessageReplyMarkup(
                chatId,
                messageId.Value,
                InlineKeyboards.TopicPicker(topics, settings.SelectedTopicId, page, totalWordCount),
                cancellationToken: cancellationToken);
        }
        catch (ApiRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to update topic picker page {MessageId}", messageId);
        }
    }

    private async Task HandleMainMenuAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

        var chatId = callbackQuery.Message?.Chat.Id;
        if (chatId is null)
        {
            return;
        }

        var userId = callbackQuery.From.Id;

        await _botClient.SendNoPreview(
            chatId.Value,
            "🏠 Главное меню",
            ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)),
            cancellationToken);
    }
}
