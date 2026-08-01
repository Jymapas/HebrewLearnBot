using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Application.Formatting;
using HebrewLearnBot.Application.UseCases.Quiz;
using HebrewLearnBot.Domain.Enums;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace HebrewLearnBot.Worker.Telegram;

public sealed class QuizPresenter
{
    private readonly ITelegramBotClient _botClient;
    private readonly BuildNextQuestion _buildNextQuestion;
    private readonly BuildQuizSummary _buildQuizSummary;
    private readonly IConversationStateRepository _conversationRepository;
    private readonly IWordRepository _wordRepository;
    private readonly IClock _clock;
    private readonly AdminGuard _adminGuard;
    private readonly ILogger<QuizPresenter> _logger;

    public QuizPresenter(
        ITelegramBotClient botClient,
        BuildNextQuestion buildNextQuestion,
        BuildQuizSummary buildQuizSummary,
        IConversationStateRepository conversationRepository,
        IWordRepository wordRepository,
        IClock clock,
        AdminGuard adminGuard,
        ILogger<QuizPresenter> logger)
    {
        _botClient = botClient;
        _buildNextQuestion = buildNextQuestion;
        _buildQuizSummary = buildQuizSummary;
        _conversationRepository = conversationRepository;
        _wordRepository = wordRepository;
        _clock = clock;
        _adminGuard = adminGuard;
        _logger = logger;
    }

    // Отправляет очередной вопрос новым сообщением и перезаписывает FSM.
    // Возвращает false, если слов не хватило (сообщение пользователю уже отправлено).
    public async Task<bool> SendNextQuestionAsync(
        long chatId,
        long userId,
        QuizMode mode,
        int? topicId,
        QuizConversationData? previous,
        CancellationToken cancellationToken)
    {
        var askedCount = previous?.AskedCount ?? 0;
        var correctCount = previous?.CorrectCount ?? 0;
        IReadOnlyList<int> recentWordIds = previous?.RecentWordIds ?? [];
        IReadOnlyList<int> wrongWordIds = previous?.WrongWordIds ?? [];

        var question = await _buildNextQuestion.ExecuteAsync(
            new BuildNextQuestionCommand(userId, mode, topicId, recentWordIds, askedCount, correctCount),
            cancellationToken);

        if (question is null)
        {
            await _conversationRepository.DeleteAsync(userId, cancellationToken);

            var notEnoughWordsText = topicId is not null
                ? "В выбранной теме слишком мало слов. Нужно минимум 4. Выберите «📚 Тема» → «Все темы» или загрузите словарь."
                : "В словаре пока недостаточно слов: для квиза нужно минимум 4. Попросите администратора загрузить словарь.";

            await _botClient.SendNoPreview(
                chatId,
                notEnoughWordsText,
                ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)),
                cancellationToken);

            return false;
        }

        var token = QuestionTokenGenerator.Generate();
        var labels = question.Options.Select(option => BidiText.ForButton(option.Label)).ToList();

        var sent = await _botClient.SendNoPreview(
            chatId,
            question.QuestionText,
            InlineKeyboards.QuizOptions(token, labels),
            cancellationToken);

        var data = new QuizConversationData
        {
            Mode = (int)mode,
            TopicId = topicId,
            QuestionToken = token,
            QuestionMessageId = sent.MessageId,
            WordId = question.CorrectWord.WordId,
            CorrectIndex = question.CorrectIndex,
            OptionWordIds = question.Options.Select(option => option.WordId).ToList(),
            OptionLabels = labels,
            AskedCount = askedCount,
            CorrectCount = correctCount,
            RecentWordIds = recentWordIds.ToList(),
            WrongWordIds = wrongWordIds.ToList()
        };

        await _conversationRepository.UpsertAsync(
            userId,
            ConversationNames.Quiz,
            QuizSteps.AwaitingAnswer,
            QuizConversationJson.Serialize(data),
            _clock.UtcNow,
            cancellationToken);

        return true;
    }

    // Показывает разбор прямо в сообщении вопроса и перекрашивает кнопки.
    public async Task ShowAnswerAsync(
        long chatId,
        QuizConversationData data,
        int chosenIndex,
        bool isCorrect,
        CancellationToken cancellationToken)
    {
        var briefs = await _wordRepository.GetBriefsByIdsAsync([data.WordId], cancellationToken);

        if (briefs.Count == 0)
        {
            _logger.LogWarning("Word {WordId} not found while showing quiz answer", data.WordId);
            return;
        }

        var chosenLabel = chosenIndex >= 0 && chosenIndex < data.OptionLabels.Count
            ? data.OptionLabels[chosenIndex]
            : null;

        var text = ReviewFormatter.Build(
            (QuizMode)data.Mode,
            briefs[0],
            isCorrect,
            chosenLabel,
            data.AskedCount + 1,
            data.CorrectCount + (isCorrect ? 1 : 0));

        try
        {
            // Один вызов EditNoPreview правит и текст, и клавиатуру разом: отдельный
            // EditMessageReplyMarkup следом вернёт 400 message is not modified.
            await _botClient.EditNoPreview(
                chatId,
                data.QuestionMessageId,
                text,
                InlineKeyboards.AnsweredQuizOptions(data.OptionLabels, data.CorrectIndex, chosenIndex),
                cancellationToken);
        }
        catch (ApiRequestException ex)
        {
            // Сообщение могли удалить, либо истекло 48-часовое окно редактирования Telegram.
            _logger.LogWarning(ex, "Failed to edit quiz message {MessageId}", data.QuestionMessageId);
        }
    }

    // Завершает тренировку: сводка, очистка FSM, возврат главного меню.
    public async Task FinishAsync(
        long chatId,
        long userId,
        QuizConversationData data,
        CancellationToken cancellationToken)
    {
        var summary = await _buildQuizSummary.ExecuteAsync(
            new BuildQuizSummaryCommand(data.AskedCount, data.CorrectCount, data.WrongWordIds),
            cancellationToken);

        await _conversationRepository.DeleteAsync(userId, cancellationToken);

        await _botClient.SendNoPreview(
            chatId,
            SummaryFormatter.Build(summary),
            ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)),
            cancellationToken);
    }
}
