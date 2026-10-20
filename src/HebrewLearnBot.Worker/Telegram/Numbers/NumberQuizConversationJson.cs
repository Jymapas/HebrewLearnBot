using System.Text.Json;

namespace HebrewLearnBot.Worker.Telegram.Numbers;

public static class NumberQuizConversationJson
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static string Serialize(NumberQuizConversationData data)
    {
        return JsonSerializer.Serialize(data, Options);
    }

    public static NumberQuizConversationData? Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<NumberQuizConversationData>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
