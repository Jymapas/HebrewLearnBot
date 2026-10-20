using HebrewLearnBot.Domain.Exceptions;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Domain.Entities;

public sealed class NumberProgress
{
    private const int LearnedCorrectThreshold = 5;
    private const int LearnedStreakThreshold = 3;

    public long UserId { get; private set; }
    public int Value { get; private set; }
    public NumberQuizMode Mode { get; private set; }
    public int CorrectCount { get; private set; }
    public int WrongCount { get; private set; }
    public int Streak { get; private set; }
    public DateTime? LastShownAtUtc { get; private set; }
    public bool? LastAnswerCorrect { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public bool IsLearned => CorrectCount >= LearnedCorrectThreshold && Streak >= LearnedStreakThreshold;

    private NumberProgress()
    {
    }

    public NumberProgress(long userId, int value, NumberQuizMode mode, DateTime createdAtUtc)
    {
        if (userId <= 0)
        {
            throw new DomainException("UserId must be positive.");
        }

        if (value is < 0 or > 99)
        {
            throw new DomainException("Value must be between 0 and 99.");
        }

        if (!Enum.IsDefined(mode))
        {
            throw new DomainException("Number quiz mode is invalid.");
        }

        if (mode == NumberQuizMode.NumberToHebrewSixOptions && value > 10)
        {
            throw new DomainException("Value must be between 0 and 10 for this mode.");
        }

        UserId = userId;
        Value = value;
        Mode = mode;
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
