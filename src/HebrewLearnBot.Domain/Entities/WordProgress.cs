using HebrewLearnBot.Domain.Exceptions;

namespace HebrewLearnBot.Domain.Entities;

public sealed class WordProgress
{
    private const int LearnedCorrectThreshold = 5;
    private const int LearnedStreakThreshold = 3;

    public long UserId { get; private set; }
    public int WordId { get; private set; }
    public int CorrectCount { get; private set; }
    public int WrongCount { get; private set; }
    public int Streak { get; private set; }
    public DateTime? LastShownAtUtc { get; private set; }
    public bool? LastAnswerCorrect { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public bool IsLearned => CorrectCount >= LearnedCorrectThreshold && Streak >= LearnedStreakThreshold;

    private WordProgress()
    {
    }

    public WordProgress(long userId, int wordId, DateTime createdAtUtc)
    {
        if (userId <= 0)
        {
            throw new DomainException("UserId must be positive.");
        }

        if (wordId <= 0)
        {
            throw new DomainException("WordId must be positive.");
        }

        UserId = userId;
        WordId = wordId;
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
