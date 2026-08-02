using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Domain.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HebrewLearnBot.Worker.Telegram;

public static class InlineKeyboards
{
    private const int TopicsPerPage = 8;

    private static readonly (QuizMode Mode, string Text)[] ModeLabels =
    [
        (QuizMode.HeToRu, "🇮🇱 → 🇷🇺 Иврит → русский"),
        (QuizMode.TranslitToRu, "🔤 → 🇷🇺 Транскрипция → русский"),
        (QuizMode.RuToHeWithTranslit, "🇷🇺 → 🇮🇱 Русский → иврит + транскр."),
        (QuizMode.RuToHe, "🇷🇺 → 🇮🇱 Русский → иврит")
    ];

    // Клавиатура до ответа: по одной кнопке в ряд — длинные подписи с ивритом
    // не влезают в две колонки, и одноколоночная раскладка не даёт Telegram
    // переставлять колонки при RTL-подписях.
    public static InlineKeyboardMarkup QuizOptions(string questionToken, IReadOnlyList<string> optionLabels)
    {
        var rows = new List<InlineKeyboardButton[]>(optionLabels.Count);

        for (var index = 0; index < optionLabels.Count; index++)
        {
            var button = InlineKeyboardButton.WithCallbackData(
                optionLabels[index],
                CallbackData.BuildQuizAnswer(questionToken, index));

            rows.Add([button]);
        }

        return new InlineKeyboardMarkup(rows);
    }

    public static InlineKeyboardMarkup AnsweredQuizOptions(
        IReadOnlyList<string> optionLabels,
        int correctIndex,
        int chosenIndex)
    {
        var rows = new List<InlineKeyboardButton[]>(optionLabels.Count);

        for (var index = 0; index < optionLabels.Count; index++)
        {
            // callback_data обязателен: инлайн-кнопка без «типа» отклоняется Bot API,
            // поэтому вместо удаления данных подставляем инертный токен.
            var button = InlineKeyboardButton.WithCallbackData(optionLabels[index], CallbackData.Noop);

            if (index == correctIndex)
            {
                button.Style = KeyboardButtonStyle.Success;   // зелёная — правильный вариант
            }
            else if (index == chosenIndex)
            {
                button.Style = KeyboardButtonStyle.Danger;    // красная — то, что выбрал пользователь
            }
            // остальные кнопки остаются без Style

            rows.Add([button]);
        }

        return new InlineKeyboardMarkup(rows);
    }

    public static InlineKeyboardMarkup AlphabetOptions(string questionToken, IReadOnlyList<string> optionLabels)
    {
        var rows = new List<InlineKeyboardButton[]>(optionLabels.Count);

        for (var index = 0; index < optionLabels.Count; index++)
        {
            var button = InlineKeyboardButton.WithCallbackData(
                optionLabels[index],
                CallbackData.BuildAlphabetAnswer(questionToken, index));

            rows.Add([button]);
        }

        return new InlineKeyboardMarkup(rows);
    }

    public static InlineKeyboardMarkup AnsweredAlphabetOptions(
        IReadOnlyList<string> optionLabels,
        int correctIndex,
        int chosenIndex)
    {
        var rows = new List<InlineKeyboardButton[]>(optionLabels.Count);

        for (var index = 0; index < optionLabels.Count; index++)
        {
            var button = InlineKeyboardButton.WithCallbackData(optionLabels[index], CallbackData.Noop);

            if (index == correctIndex)
            {
                button.Style = KeyboardButtonStyle.Success;
            }
            else if (index == chosenIndex)
            {
                button.Style = KeyboardButtonStyle.Danger;
            }

            rows.Add([button]);
        }

        return new InlineKeyboardMarkup(rows);
    }

    public static InlineKeyboardMarkup ModePicker(QuizMode current)
    {
        var rows = new List<InlineKeyboardButton[]>(ModeLabels.Length);

        foreach (var (mode, text) in ModeLabels)
        {
            var marker = mode == current ? "✅ " : "  ";
            var button = InlineKeyboardButton.WithCallbackData(marker + text, CallbackData.BuildMode(mode));
            rows.Add([button]);
        }

        return new InlineKeyboardMarkup(rows);
    }

    public static InlineKeyboardMarkup TopicPicker(
        IReadOnlyList<TopicListItem> topics,
        int? selectedTopicId,
        int page,
        int totalWordCount)
    {
        var rows = new List<InlineKeyboardButton[]>();

        // «Все темы» в домене — это отсутствие фильтра (null). Значение 0 живёт только
        // в callback_data и доменную валидацию не проходит, поэтому сверяемся именно с null.
        var allTopicsMarker = selectedTopicId is null ? "✅ " : string.Empty;
        rows.Add(
        [
            InlineKeyboardButton.WithCallbackData(
                $"{allTopicsMarker}Все темы ({totalWordCount})",
                CallbackData.BuildTopic(0))
        ]);

        var totalPages = topics.Count == 0 ? 1 : (int)Math.Ceiling(topics.Count / (double)TopicsPerPage);
        var pageTopics = topics
            .Skip(page * TopicsPerPage)
            .Take(TopicsPerPage)
            .ToList();

        for (var i = 0; i < pageTopics.Count; i += 2)
        {
            var row = new List<InlineKeyboardButton>(2);

            for (var j = i; j < i + 2 && j < pageTopics.Count; j++)
            {
                var topic = pageTopics[j];
                var marker = selectedTopicId == topic.TopicId ? "✅ " : string.Empty;
                row.Add(InlineKeyboardButton.WithCallbackData(
                    $"{marker}{topic.Name} ({topic.WordCount})",
                    CallbackData.BuildTopic(topic.TopicId)));
            }

            rows.Add(row.ToArray());
        }

        if (totalPages > 1)
        {
            var navigationRow = new List<InlineKeyboardButton>(3);

            if (page > 0)
            {
                navigationRow.Add(InlineKeyboardButton.WithCallbackData("◀️", CallbackData.BuildTopicPage(page - 1)));
            }

            navigationRow.Add(InlineKeyboardButton.WithCallbackData($"{page + 1}/{totalPages}", CallbackData.Noop));

            if (page + 1 < totalPages)
            {
                navigationRow.Add(InlineKeyboardButton.WithCallbackData("▶️", CallbackData.BuildTopicPage(page + 1)));
            }

            rows.Add(navigationRow.ToArray());
        }

        rows.Add(
        [
            InlineKeyboardButton.WithCallbackData("🔙 Главное меню", CallbackData.MainMenu)
        ]);

        return new InlineKeyboardMarkup(rows);
    }
}
