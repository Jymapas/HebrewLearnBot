using HebrewLearnBot.Application.Abstractions.Persistence;
using HebrewLearnBot.Application.Abstractions.Telegram;
using HebrewLearnBot.Application.Formatting;
using HebrewLearnBot.Application.UseCases.Words;
using HebrewLearnBot.Worker.Config;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HebrewLearnBot.Worker.Telegram.Handlers;

public sealed class DocumentHandlers
{
    private static readonly string[] AllowedExtensions = [".csv", ".tsv", ".txt"];

    private readonly ITelegramBotClient _botClient;
    private readonly AdminGuard _adminGuard;
    private readonly ImportWords _importWords;
    private readonly ITelegramFileDownloader _fileDownloader;
    private readonly IConversationStateRepository _conversationRepository;
    private readonly AppOptions _options;
    private readonly ILogger<DocumentHandlers> _logger;

    public DocumentHandlers(
        ITelegramBotClient botClient,
        AdminGuard adminGuard,
        ImportWords importWords,
        ITelegramFileDownloader fileDownloader,
        IConversationStateRepository conversationRepository,
        AppOptions options,
        ILogger<DocumentHandlers> logger)
    {
        _botClient = botClient;
        _adminGuard = adminGuard;
        _importWords = importWords;
        _fileDownloader = fileDownloader;
        _conversationRepository = conversationRepository;
        _options = options;
        _logger = logger;
    }

    public async Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        if (message.From is null || message.Document is null)
        {
            return;
        }

        var userId = message.From.Id;
        var chatId = message.Chat.Id;
        var document = message.Document;

        _logger.LogInformation("Received document {FileName} from user {UserId}", document.FileName, userId);

        var state = await _conversationRepository.GetByUserIdAsync(userId, cancellationToken);

        if (state is not { ConversationName: ConversationNames.WordImport })
        {
            await _botClient.SendNoPreview(
                chatId,
                "Чтобы загрузить словарь, нажмите «📥 Импорт словаря».",
                cancellationToken: cancellationToken);
            return;
        }

        if (!_adminGuard.IsAdmin(userId))
        {
            await _conversationRepository.DeleteAsync(userId, cancellationToken);
            await _botClient.SendNoPreview(
                chatId,
                CommandHandlers.AdminOnlyImportText,
                cancellationToken: cancellationToken);
            return;
        }

        var extension = Path.GetExtension(document.FileName ?? string.Empty).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            await _botClient.SendNoPreview(chatId, "Нужен файл .csv, .tsv или .txt.", cancellationToken: cancellationToken);
            return;
        }

        if (document.FileSize is { } fileSize && fileSize > _options.ImportMaxFileBytes)
        {
            var maxKb = _options.ImportMaxFileBytes / 1024;
            await _botClient.SendNoPreview(chatId, $"Файл больше {maxKb} КБ.", cancellationToken: cancellationToken);
            return;
        }

        try
        {
            var bytes = await _fileDownloader.DownloadAsync(document.FileId, _options.ImportMaxFileBytes, cancellationToken);

            await _botClient.SendNoPreview(chatId, "⏳ Обрабатываю файл…", cancellationToken: cancellationToken);

            var result = await _importWords.ExecuteAsync(new ImportWordsCommand(userId, bytes, _options.ImportMaxRows), cancellationToken);

            await _conversationRepository.DeleteAsync(userId, cancellationToken);

            await _botClient.SendNoPreview(
                chatId,
                ImportReportFormatter.Build(result),
                ReplyKeyboards.MainMenu(_adminGuard.IsAdmin(userId)),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process import file from user {UserId}", userId);

            await _conversationRepository.DeleteAsync(userId, cancellationToken);

            await _botClient.SendNoPreview(
                chatId,
                $"Не удалось обработать файл: {ex.Message}",
                cancellationToken: cancellationToken);
        }
    }
}
