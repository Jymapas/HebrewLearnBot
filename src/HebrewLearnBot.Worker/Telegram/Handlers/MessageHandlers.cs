using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Application.Formatting;
using HebrewLearnBot.Application.UseCases.Quiz;
using HebrewLearnBot.Application.UseCases.Settings;
using HebrewLearnBot.Application.UseCases.Statistics;
using HebrewLearnBot.Application.UseCases.Topics;
using HebrewLearnBot.Application.UseCases.Words;
using HebrewLearnBot.Domain.Entities;
using HebrewLearnBot.Domain.Enums;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HebrewLearnBot.Worker.Telegram.Handlers;

public sealed class MessageHandlers
{
    private readonly ITelegramBotClient _botClient;
    private readonly AdminGuard _adminGuard;
    private readonly QuizPresenter _quizPresenter;
    private readonly GetUserSettings _getUserSettings;
    private readonly ListTopics _listTopics;
    private readonly CountWords _countWords;
    private readonly GetUserStatistics _getUserStatistics;
    private readonly SkipQuizQuestion _skipQuizQuestion;
    private readonly IConversationStateRepository _conversationRepository;
    private readonly IClock _clock;
    private readonly ILogger<MessageHandlers> _logger;

    public MessageHandlers(
        ITelegramBotClient botClient,
        AdminGuard adminGuard,
        QuizPresenter quizPresenter,
        GetUserSettings getUserSettings,
        ListTopics listTopics,
        CountWords countWords,
        GetUserStatistics getUserStatistics,
        SkipQuizQuestion skipQuizQuestion,
        IConversationStateRepository conversationRepository,
        IClock clock,
        ILogger<MessageHandlers> logger)
    {
        _botClient = botClient;
        _adminGuard = adminGuard;
        _quizPresenter = quizPresenter;
        _getUserSettings = getUserSettings;
        _listTopics = listTopics;
        _countWords = countWords;
        _getUserStatistics = getUserStatistics;
        _skipQuizQuestion = skipQuizQuestion;
        _conversationRepository = conversationRepository;
        _clock = clock;
        _logger = logger;
    }

    public async Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        if (message.From is null)
        {
            return;
        }

        var userId = message.From.Id;
        var chatId = message.Chat.Id;
        var text = message.Text?.Trim();

        var state = await _conversationRepository.GetByUserIdAsync(userId, cancellationToken);

        if (text == MenuButtons.QuizStop)
        {
            await HandleStopAsync(chatId, userId, state, cancellationToken);
            return;
        }

        if (text == MenuButtons.QuizSkip)
        {
            await HandleSkipAsync(chatId, userId, state, cancellationToken);
            return;
        }

        if (text == MenuButtons.Cancel)
        {
            await _conversationRepository.DeleteAsync(userId, cancellationToken);
            await _botClient.SendNoPreview(
                chatId,
                CommandHandlers.CancelledText,
                ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)),
                cancellationToken);
            return;
        }

        if (text == MenuButtons.Quiz)
        {
            await StartQuizAsync(chatId, userId, cancellationToken);
            return;
        }

        if (text == MenuButtons.Mode)
        {
            var settings = await _getUserSettings.ExecuteAsync(userId, cancellationToken);
            await _botClient.SendNoPreview(
                chatId,
                CommandHandlers.ModePickerPromptText,
                InlineKeyboards.ModePicker(settings.SelectedMode),
                cancellationToken);
            return;
        }

        if (text == MenuButtons.Topic)
        {
            var settings = await _getUserSettings.ExecuteAsync(userId, cancellationToken);
            var topics = await _listTopics.ExecuteAsync(cancellationToken);
            var totalWordCount = await _countWords.ExecuteAsync(null, cancellationToken);
            await _botClient.SendNoPreview(
                chatId,
                CommandHandlers.TopicPickerPromptText,
                InlineKeyboards.TopicPicker(topics, settings.SelectedTopicId, 0, totalWordCount),
                cancellationToken);
            return;
        }

        if (text == MenuButtons.Statistics)
        {
            var statistics = await _getUserStatistics.ExecuteAsync(userId, cancellationToken);
            await _botClient.SendNoPreview(chatId, StatisticsFormatter.Build(statistics), cancellationToken: cancellationToken);
            return;
        }

        if (text == MenuButtons.Help)
        {
            await _botClient.SendNoPreview(chatId, CommandHandlers.HelpText, cancellationToken: cancellationToken);
            return;
        }

        if (text == MenuButtons.ImportWords)
        {
            await StartImportAsync(chatId, userId, cancellationToken);
            return;
        }

        if (state is { ConversationName: ConversationNames.WordImport })
        {
            await _botClient.SendNoPreview(
                chatId,
                "Пришлите файл CSV или TSV документом, а не текстом. Отменить — «↩️ Отмена».",
                cancellationToken: cancellationToken);
            return;
        }

        _logger.LogInformation("Received unhandled text message from user {UserId}", userId);

        await _botClient.SendNoPreview(
            chatId,
            "Не понимаю. Выберите действие на клавиатуре.",
            ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)),
            cancellationToken);
    }

    private async Task HandleStopAsync(long chatId, long userId, ConversationState? state, CancellationToken cancellationToken)
    {
        if (state is { ConversationName: ConversationNames.Quiz }
            && QuizConversationJson.Deserialize(state.StateJson) is { } data)
        {
            await _quizPresenter.FinishAsync(chatId, userId, data, cancellationToken);
            return;
        }

        await _botClient.SendNoPreview(
            chatId,
            CommandHandlers.NoActiveQuizText,
            ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)),
            cancellationToken);
    }

    private async Task HandleSkipAsync(long chatId, long userId, ConversationState? state, CancellationToken cancellationToken)
    {
        if (state is not { ConversationName: ConversationNames.Quiz }
            || QuizConversationJson.Deserialize(state.StateJson) is not { } data)
        {
            await _botClient.SendNoPreview(
                chatId,
                CommandHandlers.NoActiveQuizText,
                ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)),
                cancellationToken);
            return;
        }

        await _skipQuizQuestion.ExecuteAsync(new SkipQuizQuestionCommand(userId, data.WordId), cancellationToken);

        // Подсветится только правильный вариант — пользователь ничего не выбирал.
        await _quizPresenter.ShowAnswerAsync(chatId, data, chosenIndex: -1, isCorrect: false, cancellationToken);

        var updated = QuizProgressUpdater.Update(data, isCorrect: false);
        await _quizPresenter.SendNextQuestionAsync(
            chatId, userId, (QuizMode)data.Mode, data.TopicId, updated, cancellationToken);
    }

    private async Task StartQuizAsync(long chatId, long userId, CancellationToken cancellationToken)
    {
        var settings = await _getUserSettings.ExecuteAsync(userId, cancellationToken);

        var started = await _quizPresenter.SendNextQuestionAsync(
            chatId, userId, settings.SelectedMode, settings.SelectedTopicId, previous: null, cancellationToken);

        if (started)
        {
            await _botClient.SendNoPreview(chatId, CommandHandlers.QuizStartedText, ReplyKeyboards.QuizMenu(), cancellationToken);
        }
    }

    private async Task StartImportAsync(long chatId, long userId, CancellationToken cancellationToken)
    {
        if (!_adminGuard.IsAdmin(userId))
        {
            await _botClient.SendNoPreview(chatId, CommandHandlers.AdminOnlyImportText, cancellationToken: cancellationToken);
            return;
        }

        await _conversationRepository.UpsertAsync(
            userId, ConversationNames.WordImport, ImportSteps.AwaitingFile, null, _clock.UtcNow, cancellationToken);

        await _botClient.SendNoPreview(chatId, CommandHandlers.ImportInstructionsText, cancellationToken: cancellationToken);
    }
}
