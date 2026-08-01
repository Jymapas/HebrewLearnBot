using Telegram.Bot.Types.ReplyMarkups;

namespace HebrewLearnBot.Worker.Telegram;

public static class ReplyKeyboards
{
    public static ReplyKeyboardMarkup MainMenu(bool isAdmin)
    {
        var rows = new List<KeyboardButton[]>
        {
            new[]
            {
                new KeyboardButton(MenuButtons.Quiz),
                new KeyboardButton(MenuButtons.Mode)
            },
            new[]
            {
                new KeyboardButton(MenuButtons.Topic),
                new KeyboardButton(MenuButtons.Statistics)
            },
            new[]
            {
                new KeyboardButton(MenuButtons.Help)
            }
        };

        if (isAdmin)
        {
            rows.Add(
            [
                new KeyboardButton(MenuButtons.ImportWords)
            ]);
        }

        return new ReplyKeyboardMarkup(rows)
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false
        };
    }

    public static ReplyKeyboardMarkup QuizMenu()
    {
        var rows = new List<KeyboardButton[]>
        {
            new[]
            {
                new KeyboardButton(MenuButtons.QuizSkip),
                new KeyboardButton(MenuButtons.QuizStop)
            }
        };

        return new ReplyKeyboardMarkup(rows)
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false
        };
    }

    public static ReplyKeyboardRemove Remove()
    {
        return new ReplyKeyboardRemove();
    }
}
