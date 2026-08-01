namespace HebrewLearnBot.Application.Abstractions.Telegram;

public interface ITelegramFileDownloader
{
    Task<byte[]> DownloadAsync(string fileId, long maxBytes, CancellationToken cancellationToken);
}
