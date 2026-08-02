using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Time;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Formatting;
using HebrewLearnBot.Application.UseCases.Alphabet;
using HebrewLearnBot.Domain.Alphabet;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace HebrewLearnBot.Worker.Telegram;

public sealed class AlphabetQuizPresenter
{
    private readonly ITelegramBotClient _botClient;
    private readonly BuildNextLetterQuestion _buildNextLetterQuestion;
    private readonly BuildLetterQuizSummary _buildLetterQuizSummary;
    private readonly IConversationStateRepository _conversationRepository;
    private readonly IClock _clock;
    private readonly AdminGuard _adminGuard;
    private readonly ILogger<AlphabetQuizPresenter> _logger;

    public AlphabetQuizPresenter(
        ITelegramBotClient botClient,
        BuildNextLetterQuestion buildNextLetterQuestion,
        BuildLetterQuizSummary buildLetterQuizSummary,
        IConversationStateRepository conversationRepository,
        IClock clock,
        AdminGuard adminGuard,
        ILogger<AlphabetQuizPresenter> logger)
    {
        _botClient = botClient;
        _buildNextLetterQuestion = buildNextLetterQuestion;
        _buildLetterQuizSummary = buildLetterQuizSummary;
        _conversationRepository = conversationRepository;
        _clock = clock;
        _adminGuard = adminGuard;
        _logger = logger;
    }

    // Отправляет очередной вопрос новым сообщением и перезаписывает FSM.
    // Возвращает false, если букв не хватило (сообщение пользователю уже отправлено).
    public async Task<bool> SendNextQuestionAsync(
        long chatId,
        long userId,
        AlphabetQuizConversationData? previous,
        CancellationToken cancellationToken)
    {
        var askedCount = previous?.AskedCount ?? 0;
        var correctCount = previous?.CorrectCount ?? 0;
        IReadOnlyList<int> recentLetterIds = previous?.RecentLetterIds ?? [];
        IReadOnlyList<int> wrongLetterIds = previous?.WrongLetterIds ?? [];

        var question = await _buildNextLetterQuestion.ExecuteAsync(
            new BuildNextLetterQuestionCommand(userId, recentLetterIds, askedCount, correctCount),
            cancellationToken);

        if (question is null)
        {
            await _conversationRepository.DeleteAsync(userId, cancellationToken);

            await _botClient.SendNoPreview(
                chatId,
                "Не получилось подобрать вопрос по алфавиту. Попробуйте ещё раз позже.",
                ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)),
                cancellationToken);

            return false;
        }

        var token = QuestionTokenGenerator.Generate();
        var labels = question.Options.Select(option => BidiText.ForButton(option.Label)).ToList();

        var sent = await _botClient.SendNoPreview(
            chatId,
            question.QuestionText,
            InlineKeyboards.AlphabetOptions(token, labels),
            cancellationToken);

        var data = new AlphabetQuizConversationData
        {
            QuestionToken = token,
            QuestionMessageId = sent.MessageId,
            LetterId = question.CorrectLetter.LetterId,
            CorrectIndex = question.CorrectIndex,
            OptionLetterIds = question.Options.Select(option => option.LetterId).ToList(),
            OptionLabels = labels,
            AskedCount = askedCount,
            CorrectCount = correctCount,
            RecentLetterIds = recentLetterIds.ToList(),
            WrongLetterIds = wrongLetterIds.ToList()
        };

        await _conversationRepository.UpsertAsync(
            userId,
            ConversationNames.AlphabetQuiz,
            AlphabetQuizSteps.AwaitingAnswer,
            AlphabetQuizConversationJson.Serialize(data),
            _clock.UtcNow,
            cancellationToken);

        return true;
    }

    // Показывает разбор прямо в сообщении вопроса и перекрашивает кнопки.
    public async Task ShowAnswerAsync(
        long chatId,
        AlphabetQuizConversationData data,
        int chosenIndex,
        bool isCorrect,
        CancellationToken cancellationToken)
    {
        var letter = HebrewAlphabet.All.FirstOrDefault(l => l.Id == data.LetterId);

        if (letter is null)
        {
            _logger.LogWarning("Letter {LetterId} not found while showing alphabet quiz answer", data.LetterId);
            return;
        }

        var chosenLabel = chosenIndex >= 0 && chosenIndex < data.OptionLabels.Count
            ? data.OptionLabels[chosenIndex]
            : null;

        var correctBrief = new LetterBrief(letter.Id, letter.Glyph, letter.Name, letter.IsFinal);

        var text = LetterReviewFormatter.Build(
            correctBrief,
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
                InlineKeyboards.AnsweredAlphabetOptions(data.OptionLabels, data.CorrectIndex, chosenIndex),
                cancellationToken);
        }
        catch (ApiRequestException ex)
        {
            // Сообщение могли удалить, либо истекло 48-часовое окно редактирования Telegram.
            _logger.LogWarning(ex, "Failed to edit alphabet quiz message {MessageId}", data.QuestionMessageId);
        }
    }

    // Завершает тренировку: сводка, очистка FSM, возврат главного меню.
    public async Task FinishAsync(
        long chatId,
        long userId,
        AlphabetQuizConversationData data,
        CancellationToken cancellationToken)
    {
        var summary = _buildLetterQuizSummary.Execute(
            new BuildLetterQuizSummaryCommand(data.AskedCount, data.CorrectCount, data.WrongLetterIds));

        await _conversationRepository.DeleteAsync(userId, cancellationToken);

        await _botClient.SendNoPreview(
            chatId,
            LetterSummaryFormatter.Build(summary),
            ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)),
            cancellationToken);
    }
}
