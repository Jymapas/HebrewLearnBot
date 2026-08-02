using System.Text.Json;
using System.Text.Json.Serialization;

namespace HebrewLearnBot.Worker.Telegram;

public sealed class AlphabetQuizConversationData
{
    public string QuestionToken { get; set; } = string.Empty;
    public int QuestionMessageId { get; set; }
    public int LetterId { get; set; }
    public int CorrectIndex { get; set; }
    public List<int> OptionLetterIds { get; set; } = [];
    public List<string> OptionLabels { get; set; } = [];
    public int AskedCount { get; set; }
    public int CorrectCount { get; set; }
    public List<int> RecentLetterIds { get; set; } = [];
    public List<int> WrongLetterIds { get; set; } = [];
}

public static class AlphabetQuizConversationJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize(AlphabetQuizConversationData data)
    {
        return JsonSerializer.Serialize(data, Options);
    }

    public static AlphabetQuizConversationData? Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AlphabetQuizConversationData>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
