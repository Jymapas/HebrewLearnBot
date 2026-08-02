using HebrewLearnBot.Application.Quiz;

namespace HebrewLearnBot.Application.Dto;

public sealed record LetterCandidate(
    int LetterId,
    string Glyph,
    string Name,
    bool IsFinal,
    bool HasProgress,
    int CorrectCount,
    int WrongCount,
    int Streak,
    DateTime? LastShownAtUtc,
    bool? LastAnswerCorrect)
{
    public bool IsLearned => CorrectCount >= QuizConstants.LearnedCorrectThreshold
                             && Streak >= QuizConstants.LearnedStreakThreshold;
}
