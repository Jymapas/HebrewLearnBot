using HebrewLearnBot.Application.Quiz;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Application.Dto;

public sealed record QuizCandidate(
    int WordId,
    int TopicId,
    string TopicName,
    PartOfSpeech PartOfSpeech,
    Gender? Gender,
    string Hebrew,
    string HebrewNormalized,
    string? HebrewVocalized,
    string Transliteration,
    string TranslationRu,
    string TranslationNormalized,
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
