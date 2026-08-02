using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Application.Quiz;

namespace HebrewLearnBot.Application.Alphabet;

public static class LetterWeightCalculator
{
    public static double Calculate(LetterCandidate candidate, DateTime nowUtc)
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

            // Если букву ещё не показывали, считаем интервал максимально большим (равным потолку).
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
