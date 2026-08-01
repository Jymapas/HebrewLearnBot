using HebrewLearnBot.Application.Dto;

namespace HebrewLearnBot.Application.Quiz;

public static class WordWeightCalculator
{
    public static double Calculate(QuizCandidate candidate, DateTime nowUtc)
    {
        double weight;

        if (!candidate.HasProgress)
        {
            weight = QuizConstants.NewWordWeight;
        }
        else
        {
            var accuracy = (1.0 + QuizConstants.WrongWeightFactor * candidate.WrongCount)
                / (1.0 + QuizConstants.CorrectWeightFactor * candidate.CorrectCount);

            // Если слово ещё не показывали, считаем интервал максимально большим (равным потолку).
            var days = candidate.LastShownAtUtc is null
                ? QuizConstants.SpacingCapDays
                : (nowUtc - candidate.LastShownAtUtc.Value).TotalDays;
            var clampedDays = Math.Min(Math.Max(days, 0.0), QuizConstants.SpacingCapDays);
            var spacing = 1.0 + clampedDays / QuizConstants.SpacingDivisorDays;

            var lastMiss = candidate.LastAnswerCorrect == false
                ? QuizConstants.LastMissMultiplier
                : 1.0;

            var damp = candidate.IsLearned
                ? QuizConstants.LearnedDamping
                : 1.0;

            weight = accuracy * spacing * lastMiss * damp;
        }

        return Math.Clamp(weight, QuizConstants.MinWeight, QuizConstants.MaxWeight);
    }
}
