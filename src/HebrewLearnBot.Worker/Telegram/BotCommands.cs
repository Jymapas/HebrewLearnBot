using Telegram.Bot.Types;

namespace HebrewLearnBot.Worker.Telegram;

public static class BotCommands
{
    public const string Start = "/start";
    public const string Menu = "/menu";
    public const string Quiz = "/quiz";
    public const string Stop = "/stop";
    public const string Mode = "/mode";
    public const string Topic = "/topic";
    public const string Stats = "/stats";
    public const string Import = "/import";
    public const string Cancel = "/cancel";
    public const string Help = "/help";

    public static readonly string[] All =
    [
        Start,
        Menu,
        Quiz,
        Stop,
        Mode,
        Topic,
        Stats,
        Import,
        Cancel,
        Help,
    ];

    private static readonly Dictionary<string, string> CustomDescriptions = new(StringComparer.OrdinalIgnoreCase)
    {
        [Start] = "Главное меню",
        [Menu] = "Главное меню",
        [Quiz] = "Начать тренировку",
        [Stop] = "Завершить тренировку",
        [Mode] = "Выбрать режим квиза",
        [Topic] = "Выбрать тему",
        [Stats] = "Статистика",
        [Import] = "Импорт словаря (только админ)",
        [Cancel] = "Отменить текущее действие",
        [Help] = "Помощь",
    };

    public static IReadOnlyList<BotCommand> AsBotCommands() =>
        All
            .Select(c =>
            {
                var cmd = c.TrimStart('/');
                if (CustomDescriptions.TryGetValue(c, out var desc) && !string.IsNullOrWhiteSpace(desc))
                {
                    return new BotCommand { Command = cmd, Description = desc };
                }

                return null;
            })
            .Where(static c => c is not null)
            .Select(static c => c!)
            .ToArray();
}
