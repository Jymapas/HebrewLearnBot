using System.Text.Json;
using System.Text.Json.Serialization;

namespace HebrewLearnBot.Worker.Telegram;

public sealed class QuizConversationData
{
    public int Mode { get; set; }
    public int? TopicId { get; set; }
    public string QuestionToken { get; set; } = string.Empty;
    public int QuestionMessageId { get; set; }
    public int WordId { get; set; }
    public int CorrectIndex { get; set; }
    public List<int> OptionWordIds { get; set; } = [];
    public List<string> OptionLabels { get; set; } = [];
    public int AskedCount { get; set; }
    public int CorrectCount { get; set; }
    public List<int> RecentWordIds { get; set; } = [];
    public List<int> WrongWordIds { get; set; } = [];
}

public static class QuizConversationJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize(QuizConversationData data)
    {
        return JsonSerializer.Serialize(data, Options);
    }

    public static QuizConversationData? Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<QuizConversationData>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
