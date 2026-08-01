using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Application.Formatting;
using HebrewLearnBot.Application.UseCases.Settings;
using HebrewLearnBot.Application.UseCases.Statistics;
using HebrewLearnBot.Application.UseCases.Topics;
using HebrewLearnBot.Application.UseCases.Words;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HebrewLearnBot.Worker.Telegram.Handlers;

public sealed class CommandHandlers
{
    internal const string HelpText =
        "ℹ️ Помощь\n\n" +
        "Режимы квиза (кнопка «⚙️ Режим»):\n" +
        "🇮🇱 → 🇷🇺 Иврит → русский\n" +
        "🔤 → 🇷🇺 Транскрипция → русский\n" +
        "🇷🇺 → 🇮🇱 Русский → иврит + транскрипция\n" +
        "🇷🇺 → 🇮🇱 Русский → иврит\n\n" +
        "Кнопки:\n" +
        "🎯 Квиз — начать тренировку\n" +
        "⏭ Пропустить — пропустить текущий вопрос\n" +
        "⏹ Стоп — завершить тренировку и увидеть сводку\n" +
        "⚙️ Режим — выбрать режим вопросов\n" +
        "📚 Тема — выбрать тему слов\n" +
        "📊 Статистика — посмотреть прогресс\n" +
        "ℹ️ Помощь — это сообщение\n\n" +
        "Слова, в которых вы чаще ошибаетесь, выпадают в квизе чаще остальных.";

    internal const string QuizStartedText = "Тренировка началась. Отвечайте кнопками.";

    internal const string NoActiveQuizText = "Сейчас нет активной тренировки.";

    internal const string CancelledText = "Отменено.";

    internal const string ModePickerPromptText = "Выберите режим тренировки:";

    internal const string TopicPickerPromptText = "Выберите тему:";

    internal const string AdminOnlyImportText = "Импорт словаря доступен только администратору.";

    internal const string ImportInstructionsText =
        "📥 Пришлите файл CSV или TSV со словами.\n\n" +
        "Первая строка — заголовок. Обязательные колонки: иврит, транскрипция, перевод, тема.\n" +
        "Необязательные: никуд, род, часть речи, ключ.\n\n" +
        "Пример:\n" +
        "hebrew;translit;ru;topic;gender\n" +
        "שולחן;shulkhan;стол;Дом;м\n\n" +
        "Повторная загрузка обновляет уже существующие слова, а не дублирует их.\n" +
        "Отменить — «↩️ Отмена».";

    private readonly ITelegramBotClient _botClient;
    private readonly AdminGuard _adminGuard;
    private readonly QuizPresenter _quizPresenter;
    private readonly GetUserSettings _getUserSettings;
    private readonly ListTopics _listTopics;
    private readonly CountWords _countWords;
    private readonly GetUserStatistics _getUserStatistics;
    private readonly IConversationStateRepository _conversationRepository;
    private readonly IClock _clock;
    private readonly ILogger<CommandHandlers> _logger;

    public CommandHandlers(
        ITelegramBotClient botClient,
        AdminGuard adminGuard,
        QuizPresenter quizPresenter,
        GetUserSettings getUserSettings,
        ListTopics listTopics,
        CountWords countWords,
        GetUserStatistics getUserStatistics,
        IConversationStateRepository conversationRepository,
        IClock clock,
        ILogger<CommandHandlers> logger)
    {
        _botClient = botClient;
        _adminGuard = adminGuard;
        _quizPresenter = quizPresenter;
        _getUserSettings = getUserSettings;
        _listTopics = listTopics;
        _countWords = countWords;
        _getUserStatistics = getUserStatistics;
        _conversationRepository = conversationRepository;
        _clock = clock;
        _logger = logger;
    }

    public async Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message.Text) || message.From is null)
        {
            return;
        }

        var command = message.Text.Split(' ', '\n')[0].Trim();
        var atIndex = command.IndexOf('@');
        if (atIndex > 0)
        {
            command = command[..atIndex];
        }

        var userId = message.From.Id;
        var chatId = message.Chat.Id;

        _logger.LogInformation("Handling command {Command} from user {UserId}", command, userId);

        switch (command)
        {
            case BotCommands.Start:
            case BotCommands.Menu:
                await ShowMainMenuAsync(chatId, userId, cancellationToken);
                break;

            case BotCommands.Quiz:
                await StartQuizAsync(chatId, userId, cancellationToken);
                break;

            case BotCommands.Stop:
                await StopQuizAsync(chatId, userId, cancellationToken);
                break;

            case BotCommands.Mode:
                await ShowModePickerAsync(chatId, userId, cancellationToken);
                break;

            case BotCommands.Topic:
                await ShowTopicPickerAsync(chatId, userId, cancellationToken);
                break;

            case BotCommands.Stats:
                await ShowStatisticsAsync(chatId, userId, cancellationToken);
                break;

            case BotCommands.Import:
                await StartImportAsync(chatId, userId, cancellationToken);
                break;

            case BotCommands.Cancel:
                await CancelAsync(chatId, userId, cancellationToken);
                break;

            case BotCommands.Help:
                await ShowHelpAsync(chatId, cancellationToken);
                break;

            default:
                await _botClient.SendNoPreview(
                    chatId,
                    $"Неизвестная команда. Доступные: {string.Join(", ", BotCommands.All)}",
                    cancellationToken: cancellationToken);
                break;
        }
    }

    private async Task ShowMainMenuAsync(long chatId, long userId, CancellationToken cancellationToken)
    {
        await _conversationRepository.DeleteAsync(userId, cancellationToken);

        await _botClient.SendNoPreview(
            chatId,
            "👋 Привет! Я помогу тебе выучить ивритские слова.\n\nВыбери действие на клавиатуре ниже.",
            ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)),
            cancellationToken);
    }

    private async Task ShowHelpAsync(long chatId, CancellationToken cancellationToken)
    {
        await _botClient.SendNoPreview(chatId, HelpText, cancellationToken: cancellationToken);
    }

    private async Task StartQuizAsync(long chatId, long userId, CancellationToken cancellationToken)
    {
        var settings = await _getUserSettings.ExecuteAsync(userId, cancellationToken);

        var started = await _quizPresenter.SendNextQuestionAsync(
            chatId, userId, settings.SelectedMode, settings.SelectedTopicId, previous: null, cancellationToken);

        // Клавиатуру квиза шлём после вопроса: иначе ReplyKeyboardMarkup прилетит
        // раньше и вопрос окажется ниже него в чате.
        if (started)
        {
            await _botClient.SendNoPreview(chatId, QuizStartedText, ReplyKeyboards.QuizMenu(), cancellationToken);
        }
    }

    private async Task StopQuizAsync(long chatId, long userId, CancellationToken cancellationToken)
    {
        var state = await _conversationRepository.GetByUserIdAsync(userId, cancellationToken);

        if (state is { ConversationName: ConversationNames.Quiz }
            && QuizConversationJson.Deserialize(state.StateJson) is { } data)
        {
            await _quizPresenter.FinishAsync(chatId, userId, data, cancellationToken);
            return;
        }

        await _botClient.SendNoPreview(
            chatId,
            NoActiveQuizText,
            ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)),
            cancellationToken);
    }

    private async Task ShowModePickerAsync(long chatId, long userId, CancellationToken cancellationToken)
    {
        var settings = await _getUserSettings.ExecuteAsync(userId, cancellationToken);

        await _botClient.SendNoPreview(
            chatId,
            ModePickerPromptText,
            InlineKeyboards.ModePicker(settings.SelectedMode),
            cancellationToken);
    }

    private async Task ShowTopicPickerAsync(long chatId, long userId, CancellationToken cancellationToken)
    {
        var settings = await _getUserSettings.ExecuteAsync(userId, cancellationToken);
        var topics = await _listTopics.ExecuteAsync(cancellationToken);
        var totalWordCount = await _countWords.ExecuteAsync(null, cancellationToken);

        await _botClient.SendNoPreview(
            chatId,
            TopicPickerPromptText,
            InlineKeyboards.TopicPicker(topics, settings.SelectedTopicId, 0, totalWordCount),
            cancellationToken);
    }

    private async Task ShowStatisticsAsync(long chatId, long userId, CancellationToken cancellationToken)
    {
        var statistics = await _getUserStatistics.ExecuteAsync(userId, cancellationToken);

        await _botClient.SendNoPreview(chatId, StatisticsFormatter.Build(statistics), cancellationToken: cancellationToken);
    }

    private async Task StartImportAsync(long chatId, long userId, CancellationToken cancellationToken)
    {
        if (!_adminGuard.IsAdmin(userId))
        {
            await _botClient.SendNoPreview(chatId, AdminOnlyImportText, cancellationToken: cancellationToken);
            return;
        }

        await _conversationRepository.UpsertAsync(
            userId, ConversationNames.WordImport, ImportSteps.AwaitingFile, null, _clock.UtcNow, cancellationToken);

        await _botClient.SendNoPreview(chatId, ImportInstructionsText, cancellationToken: cancellationToken);
    }

    private async Task CancelAsync(long chatId, long userId, CancellationToken cancellationToken)
    {
        await _conversationRepository.DeleteAsync(userId, cancellationToken);

        await _botClient.SendNoPreview(
            chatId,
            CancelledText,
            ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)),
            cancellationToken);
    }
}
