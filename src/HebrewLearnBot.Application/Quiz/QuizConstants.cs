namespace HebrewLearnBot.Application.Quiz;

public static class QuizConstants
{
    public const int OptionCount = 4;
    public const int MinimumOptionCount = 2;
    public const int MinimumWordsForQuiz = 4;
    public const int RecentWindowSize = 10;
    public const int MaxWrongWordsTracked = 100;

    public const int LearnedCorrectThreshold = 5;
    public const int LearnedStreakThreshold = 3;

    public const double NewWordWeight = 6.0;
    public const double MinWeight = 0.05;
    public const double MaxWeight = 50.0;
    public const double RecentPenalty = 0.02;
    public const double SpacingCapDays = 14.0;
    public const double SpacingDivisorDays = 7.0;
    public const double LastMissMultiplier = 1.5;
    public const double LearnedDamping = 0.35;
    public const double WrongWeightFactor = 2.0;
    public const double CorrectWeightFactor = 1.5;
}
