using HebrewLearnBot.Domain.Enums;
using HebrewLearnBot.Domain.Exceptions;

namespace HebrewLearnBot.Domain.Entities;

public sealed class UserSettings
{
    public long UserId { get; private set; }
    public QuizMode SelectedMode { get; private set; }
    public int? SelectedTopicId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private UserSettings()
    {
    }

    public UserSettings(long userId, DateTime createdAtUtc)
    {
        if (userId <= 0)
        {
            throw new DomainException("UserId must be positive.");
        }

        UserId = userId;
        SelectedMode = QuizMode.HeToRu;
        SelectedTopicId = null;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public void SelectMode(QuizMode mode, DateTime updatedAtUtc)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new DomainException("Unknown quiz mode.");
        }

        SelectedMode = mode;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SelectTopic(int? topicId, DateTime updatedAtUtc)
    {
        if (topicId.HasValue && topicId.Value <= 0)
        {
            throw new DomainException("TopicId must be positive when provided.");
        }

        SelectedTopicId = topicId;
        UpdatedAtUtc = updatedAtUtc;
    }
}
