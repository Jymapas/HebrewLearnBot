namespace HebrewLearnBot.Application.Dto;

public sealed record QuizSummary(int AskedCount, int CorrectCount, IReadOnlyList<WordBrief> WrongWords);
