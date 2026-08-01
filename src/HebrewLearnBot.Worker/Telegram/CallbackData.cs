using System.Globalization;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Worker.Telegram;

public static class CallbackData
{
    public const string QuizAnswerPrefix = "q:";
    public const string Noop = "x";
    public const string ModePrefix = "qm:";
    public const string TopicPrefix = "qt:";
    public const string TopicPagePrefix = "qtp:";
    public const string MenuPrefix = "m:";
    public const string MainMenu = "m:main";

    public static string BuildQuizAnswer(string questionToken, int optionIndex)
    {
        return $"{QuizAnswerPrefix}{questionToken}:{optionIndex}";
    }

    public static bool TryParseQuizAnswer(string? data, out string questionToken, out int optionIndex)
    {
        questionToken = string.Empty;
        optionIndex = 0;

        if (string.IsNullOrEmpty(data) || !data.StartsWith(QuizAnswerPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var remainder = data[QuizAnswerPrefix.Length..];
        var separatorIndex = remainder.LastIndexOf(':');
        if (separatorIndex <= 0 || separatorIndex == remainder.Length - 1)
        {
            return false;
        }

        var token = remainder[..separatorIndex];
        var indexPart = remainder[(separatorIndex + 1)..];

        if (!int.TryParse(indexPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedIndex))
        {
            return false;
        }

        questionToken = token;
        optionIndex = parsedIndex;
        return true;
    }

    public static string BuildMode(QuizMode mode)
    {
        return $"{ModePrefix}{(int)mode}";
    }

    public static bool TryParseMode(string? data, out QuizMode mode)
    {
        mode = default;

        if (string.IsNullOrEmpty(data) || !data.StartsWith(ModePrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var remainder = data[ModePrefix.Length..];
        if (!int.TryParse(remainder, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return false;
        }

        if (!Enum.IsDefined(typeof(QuizMode), value))
        {
            return false;
        }

        mode = (QuizMode)value;
        return true;
    }

    public static string BuildTopic(int topicId)
    {
        return $"{TopicPrefix}{topicId}";
    }

    public static bool TryParseTopic(string? data, out int topicId)
    {
        topicId = 0;

        if (string.IsNullOrEmpty(data) || !data.StartsWith(TopicPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var remainder = data[TopicPrefix.Length..];
        return int.TryParse(remainder, NumberStyles.Integer, CultureInfo.InvariantCulture, out topicId);
    }

    public static string BuildTopicPage(int page)
    {
        return $"{TopicPagePrefix}{page}";
    }

    public static bool TryParseTopicPage(string? data, out int page)
    {
        page = 0;

        if (string.IsNullOrEmpty(data) || !data.StartsWith(TopicPagePrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var remainder = data[TopicPagePrefix.Length..];
        return int.TryParse(remainder, NumberStyles.Integer, CultureInfo.InvariantCulture, out page);
    }
}
