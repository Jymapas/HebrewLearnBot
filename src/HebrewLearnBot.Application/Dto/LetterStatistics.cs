namespace HebrewLearnBot.Application.Dto;

public sealed record LetterStatistics(
    int TotalLetters,
    int SeenLetters,
    int LearnedLetters,
    int TotalAnswers,
    int CorrectAnswers,
    IReadOnlyList<LetterBrief> TopProblemLetters);
