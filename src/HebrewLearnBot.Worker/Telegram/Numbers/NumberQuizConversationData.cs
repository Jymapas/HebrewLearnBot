using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Worker.Telegram.Numbers;

public sealed class NumberQuizConversationData
{
    public long ChatId { get; set; }
    public int MessageId { get; set; }
    public string Token { get; set; } = string.Empty;
    public NumberQuizMode Mode { get; set; }
    public int Value { get; set; }
    public int CorrectIndex { get; set; }
    public List<string> Labels { get; set; } = [];
    public int AskedCount { get; set; }
    public int CorrectCount { get; set; }
    public List<int> RecentNumbers { get; set; } = [];
    public List<int> WrongNumbers { get; set; } = [];
}
