namespace HebrewLearnBot.Application.Dto;

public sealed record LetterQuizQuestion(
    LetterBrief CorrectLetter,
    string QuestionText,
    IReadOnlyList<LetterQuizOption> Options,
    int CorrectIndex);
