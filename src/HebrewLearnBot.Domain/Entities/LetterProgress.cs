using HebrewLearnBot.Domain.Exceptions;

namespace HebrewLearnBot.Domain.Entities;

public sealed class LetterProgress
{
    private const int LearnedCorrectThreshold = 5;
    private const int LearnedStreakThreshold = 3;

    public long UserId { get; private set; }
    public int LetterId { get; private set; }
    public int CorrectCount { get; private set; }
    public int WrongCount { get; private set; }
    public int Streak { get; private set; }
    public DateTime? LastShownAtUtc { get; private set; }
    public bool? LastAnswerCorrect { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public bool IsLearned => CorrectCount >= LearnedCorrectThreshold && Streak >= LearnedStreakThreshold;

    private LetterProgress()
    {
    }

    public LetterProgress(long userId, int letterId, DateTime createdAtUtc)
    {
        if (userId <= 0)
        {
            throw new DomainException("UserId must be positive.");
        }

        if (letterId <= 0)
        {
            throw new DomainException("LetterId must be positive.");
        }

        UserId = userId;
        LetterId = letterId;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public void RegisterAnswer(bool isCorrect, DateTime answeredAtUtc)
    {
        if (isCorrect)
        {
            CorrectCount++;
            Streak++;
        }
        else
        {
            WrongCount++;
            Streak = 0;
        }

        LastAnswerCorrect = isCorrect;
        LastShownAtUtc = answeredAtUtc;
        UpdatedAtUtc = answeredAtUtc;
    }

    public void RegisterSkip(DateTime skippedAtUtc)
    {
        Streak = 0;
        LastAnswerCorrect = null;
        LastShownAtUtc = skippedAtUtc;
        UpdatedAtUtc = skippedAtUtc;
    }
}
