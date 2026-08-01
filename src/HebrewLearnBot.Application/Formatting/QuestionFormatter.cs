using System.Globalization;
using System.Text;
using HebrewLearnBot.Application.Dto;
using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Application.Formatting;

public static class QuestionFormatter
{
    public static string BuildQuestionText(QuizMode mode, QuizCandidate correct, int askedCount, int correctCount)
    {
        var wording = GetWording(mode);
        var word = GetQuestionWord(mode, correct);

        var builder = new StringBuilder();

        builder.Append(BidiText.Line(
            $"📊 Вопрос {(askedCount + 1).ToString(CultureInfo.InvariantCulture)} · верно {correctCount.ToString(CultureInfo.InvariantCulture)}"));
        builder.Append('\n');
        builder.Append('\n');
        builder.Append(BidiText.Line($"{wording} {word}"));

        return builder.ToString();
    }

    public static string BuildOptionLabel(QuizMode mode, QuizCandidate candidate)
    {
        return mode switch
        {
            QuizMode.HeToRu => candidate.TranslationRu,
            QuizMode.TranslitToRu => candidate.TranslationRu,
            QuizMode.RuToHeWithTranslit => $"{BidiText.Isolate(candidate.Hebrew)} ({candidate.Transliteration})",
            QuizMode.RuToHe => BidiText.Isolate(candidate.Hebrew),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    private static string GetWording(QuizMode mode)
    {
        return mode switch
        {
            QuizMode.HeToRu => "Как переводится?",
            QuizMode.TranslitToRu => "Как переводится?",
            QuizMode.RuToHeWithTranslit => "Переведите на иврит:",
            QuizMode.RuToHe => "Переведите на иврит:",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    private static string GetQuestionWord(QuizMode mode, QuizCandidate correct)
    {
        return mode switch
        {
            QuizMode.HeToRu => BidiText.Isolate(correct.Hebrew),
            QuizMode.TranslitToRu => correct.Transliteration,
            QuizMode.RuToHeWithTranslit => correct.TranslationRu,
            QuizMode.RuToHe => correct.TranslationRu,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}
