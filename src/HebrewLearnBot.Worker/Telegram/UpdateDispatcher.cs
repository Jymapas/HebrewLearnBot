using HebrewLearnBot.Worker.Telegram.Handlers;
using HebrewLearnBot.Worker.Telegram.Numbers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace HebrewLearnBot.Worker.Telegram;

public sealed class UpdateDispatcher
{
    private static readonly SemaphoreSlim[] UserGates = Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1, 1)).ToArray();
    private static readonly TimeSpan ErrorRetryDelay = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UpdateDispatcher> _logger;

    public UpdateDispatcher(IServiceScopeFactory scopeFactory, ILogger<UpdateDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Фиксированное число блокировок не накапливает ключи пользователей;
        // ответы и навигация одного пользователя обрабатываются последовательно.
        var userId = update.CallbackQuery?.From.Id ?? update.Message?.From?.Id ?? 0;
        var gate = UserGates[(int)((ulong)userId % (ulong)UserGates.Length)];
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (update.CallbackQuery is { } callbackQuery)
            {
                using var scope = _scopeFactory.CreateScope();
                var numbers = scope.ServiceProvider.GetRequiredService<NumberQuizHandler>();
                if (await numbers.TryHandleCallbackAsync(callbackQuery, cancellationToken))
                {
                    return;
                }
                var callbackHandlers = scope.ServiceProvider.GetRequiredService<CallbackHandlers>();
                await callbackHandlers.HandleAsync(callbackQuery, cancellationToken);
                return;
            }

            if (update.Message is { } message)
            {
                using var scope = _scopeFactory.CreateScope();

                var numbers = scope.ServiceProvider.GetRequiredService<NumberQuizHandler>();
                if (await numbers.TryHandleMessageAsync(message, cancellationToken))
                {
                    return;
                }

                if (message.Document is not null)
                {
                    var documentHandlers = scope.ServiceProvider.GetRequiredService<DocumentHandlers>();
                    await documentHandlers.HandleAsync(message, cancellationToken);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(message.Text) && message.Text.StartsWith('/'))
                {
                    var commandHandlers = scope.ServiceProvider.GetRequiredService<CommandHandlers>();
                    await commandHandlers.HandleAsync(message, cancellationToken);
                    if (message.From is not null && message.Text.Split(' ', '\n')[0].Split('@')[0] == BotCommands.Stats)
                    {
                        await numbers.AppendStatisticsAsync(message.Chat.Id, message.From.Id, cancellationToken);
                    }
                    return;
                }

                var messageHandlers = scope.ServiceProvider.GetRequiredService<MessageHandlers>();
                await messageHandlers.HandleAsync(message, cancellationToken);
                if (message.From is not null && message.Text?.Trim() == MenuButtons.Statistics)
                {
                    await numbers.AppendStatisticsAsync(message.Chat.Id, message.From.Id, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing update {UpdateId}", update.Id);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception.Message;
        if (errorMessage.Contains("409") || errorMessage.Contains("Conflict"))
        {
            _logger.LogWarning(
                exception,
                "Telegram Bot API error 409: another bot instance is already running. Stop other instances or wait before restarting.");
        }
        else
        {
            _logger.LogError(exception, "Telegram polling error from source {Source}", source);
        }

        // Telegram.Bot повторяет getUpdates сразу после ошибки. При устойчивом сбое
        // (отозванный токен, пропавшая сеть) это около семи записей в секунду и десятки
        // мегабайт логов в сутки, поэтому притормаживаем цикл повторов.
        try
        {
            await Task.Delay(ErrorRetryDelay, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Бот останавливается — выходим молча.
        }
    }
}
