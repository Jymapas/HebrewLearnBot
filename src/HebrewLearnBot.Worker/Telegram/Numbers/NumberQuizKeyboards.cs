using HebrewLearnBot.Application.Formatting;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace HebrewLearnBot.Worker.Telegram.Numbers;

public static class NumberQuizKeyboards
{
    public const string Prefix = "num:";

    public static InlineKeyboardMarkup Menu(string token, string page, bool isPrivateChat)
    {
        (string Label, string Action)[] choices = page switch
        {
            "cards" =>
            [
                ("Иврит → число", "1"),
                ("Транскрипция → число", "2"),
                ("Число → иврит + транскрипция", "3"),
                ("Число → иврит", "4"),
                ("🔙 Назад", "menu")
            ],
            "input" =>
            [
                ("Иврит → ввести число", "5"),
                ("Транскрипция → ввести число", "6"),
                ("🔙 Назад", "menu")
            ],
            _ =>
            [
                ("Карточки 0–99", "cards"),
                ("Ввести число 0–99", "input"),
                ("Число → иврит 0–10 · 6 вариантов", "7")
            ]
        };
        var rows = choices.Select(choice => new[]
        {
            InlineKeyboardButton.WithCallbackData(BidiText.ForButton(choice.Label), $"{Prefix}{token}:{choice.Action}")
        }).ToList();

        if (page == "menu" && isPrivateChat)
        {
            rows.Add([InlineKeyboardButton.WithWebApp(
                BidiText.ForButton("Найди пару · 0–10"),
                new WebAppInfo { Url = "https://jymapas.github.io/HebrewLearnBot/" })]);
        }

        return new InlineKeyboardMarkup(rows);
    }

    public static InlineKeyboardMarkup Options(NumberQuizConversationData data, int? chosenIndex)
    {
        return new InlineKeyboardMarkup(data.Labels.Select((label, index) =>
        {
            var button = InlineKeyboardButton.WithCallbackData(
                BidiText.ForButton(label), chosenIndex.HasValue ? CallbackData.Noop : $"{Prefix}{data.Token}:a{index}");
            if (chosenIndex.HasValue)
            {
                if (index == data.CorrectIndex)
                {
                    button.Style = KeyboardButtonStyle.Success;
                }
                else if (index == chosenIndex.Value)
                {
                    button.Style = KeyboardButtonStyle.Danger;
                }
            }

            return new[] { button };
        }));
    }
}
