using HebrewLearnBot.Application.Abstractions.Telegram;
using Telegram.Bot;

namespace HebrewLearnBot.Infrastructure.Telegram;

public sealed class TelegramFileDownloader : ITelegramFileDownloader
{
    private readonly ITelegramBotClient _botClient;

    public TelegramFileDownloader(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task<byte[]> DownloadAsync(string fileId, long maxBytes, CancellationToken cancellationToken)
    {
        var file = await _botClient.GetFile(fileId, cancellationToken);

        if (file.FileSize is > 0 && file.FileSize > maxBytes)
        {
            throw new InvalidOperationException("File is too large.");
        }

        using var stream = new MemoryStream();
        await _botClient.DownloadFile(file.FilePath!, stream, cancellationToken);

        return stream.ToArray();
    }
}
