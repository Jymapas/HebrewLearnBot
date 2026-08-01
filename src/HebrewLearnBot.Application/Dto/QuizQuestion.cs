using HebrewLearnBot.Domain.Enums;

namespace HebrewLearnBot.Application.Dto;

public sealed record QuizQuestion(
    QuizMode Mode,
    WordBrief CorrectWord,
    string QuestionText,
    IReadOnlyList<QuizOption> Options,
    int CorrectIndex);
