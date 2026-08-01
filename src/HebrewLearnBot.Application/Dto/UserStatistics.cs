namespace HebrewLearnBot.Application.Dto;

public sealed record UserStatistics(
    int TotalWords,
    int SeenWords,
    int LearnedWords,
    int TotalAnswers,
    int CorrectAnswers,
    IReadOnlyList<WordBrief> TopProblemWords);
